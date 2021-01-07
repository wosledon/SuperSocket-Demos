using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.ProtoBase;
using Microsoft.Extensions.Logging;

namespace SuperSocket.Command
{

    public class CommandMiddleware<TKey, TPackageInfo> : CommandMiddleware<TKey, TPackageInfo, TPackageInfo>
        where TPackageInfo : class, IKeyedPackageInfo<TKey>
    {

        class TransparentMapper : IPackageMapper<TPackageInfo, TPackageInfo>
        {
            public TPackageInfo Map(TPackageInfo package)
            {
                return package;
            }
        }
        /// <summary>
        /// 初始化命令行中间件
        /// </summary>
        /// <param name="serviceProvider">ServiceProvider</param>
        /// <param name="commandOptions">命令行设置</param>
        public CommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions)
            : base(serviceProvider, commandOptions)
        {

        }
        /// <summary>
        /// 创建数据包映射
        /// </summary>
        /// <param name="serviceProvider">ServiceProvider</param>
        /// <returns></returns>
        protected override IPackageMapper<TPackageInfo, TPackageInfo> CreatePackageMapper(IServiceProvider serviceProvider)
        {
            return new TransparentMapper();
        }
    }

    public class CommandMiddleware<TKey, TNetPackageInfo, TPackageInfo> : MiddlewareBase, IPackageHandler<TNetPackageInfo>
        where TPackageInfo : class, IKeyedPackageInfo<TKey>
        where TNetPackageInfo : class
    {
        private Dictionary<TKey, ICommandSet> _commands;

        private ILogger _logger;
        /// <summary>
        /// 数据包映射
        /// </summary>
        protected IPackageMapper<TNetPackageInfo, TPackageInfo> PackageMapper { get; private set; }
        /// <summary>
        /// 初始化命令中间件
        /// </summary>
        /// <param name="serviceProvider">ServiceProvider</param>
        /// <param name="commandOptions">命令选项</param>
        public CommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions)
            : this(serviceProvider, commandOptions, null)
        {

        }
        /// <summary>
        /// 初始化命令中间件
        /// </summary>
        /// <param name="serviceProvider">ServiceProvider</param>
        /// <param name="commandOptions">命令选项</param>
        /// <param name="packageMapper">包映射</param>
        public CommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions, IPackageMapper<TNetPackageInfo, TPackageInfo> packageMapper)
        {
            _logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger("CommandMiddleware");

            var sessionFactory = serviceProvider.GetService<ISessionFactory>();
            var sessionType = sessionFactory == null ? typeof(IAppSession) : sessionFactory.SessionType;

            var commandInterfaces = new List<CommandTypeInfo>();
            var commandSetFactories = new List<ICommandSetFactory>();

            RegisterCommandInterfaces(commandInterfaces, commandSetFactories, serviceProvider, sessionType, typeof(TPackageInfo));


            if (sessionType != typeof(IAppSession))
            {
                RegisterCommandInterfaces(commandInterfaces, commandSetFactories, serviceProvider, typeof(IAppSession), typeof(TPackageInfo));
            }

            var knownInterfaces = new Type[] { typeof(IKeyedPackageInfo<TKey>) };

            foreach (var f in typeof(TPackageInfo).GetTypeInfo().GetInterfaces())
            {
                if (knownInterfaces.Contains(f))
                    continue;

                RegisterCommandInterfaces(commandInterfaces, commandSetFactories, serviceProvider, sessionType, f, true);

                if (sessionType != typeof(IAppSession))
                {
                    RegisterCommandInterfaces(commandInterfaces, commandSetFactories, serviceProvider, typeof(IAppSession), f, true);
                }
            }

            commandSetFactories.AddRange(commandOptions.Value.GetCommandTypes(t => true).Select((t) => 
            {
                if (t.IsAbstract)
                    return null;

                for (var i = 0; i < commandInterfaces.Count; i++)
                {
                    var face = commandInterfaces[i];

                    if (face.CommandType.IsAssignableFrom(t))
                        return face.CreateCommandSetFactory(t);
                }

                return null;
            }).Where(t => t != null));

            
            var commands = commandSetFactories.Select(t => t.Create(serviceProvider, commandOptions.Value));
            var comparer = serviceProvider.GetService<IEqualityComparer<TKey>>();

            var commandDict = comparer == null ?
                new Dictionary<TKey, ICommandSet>() : new Dictionary<TKey, ICommandSet>(comparer);

            foreach (var cmd in commands)
            {
                if (commandDict.ContainsKey(cmd.Key))
                {
                    var error = $"Duplicated command with Key {cmd.Key} is found: {cmd.ToString()}";
                    _logger.LogError(error);
                    throw new Exception(error);
                }

                commandDict.Add(cmd.Key, cmd);
                _logger.LogDebug($"The command with key {cmd.Key} is registered: {cmd.ToString()}");
            }

            _commands = commandDict;
            
            PackageMapper = packageMapper != null ? packageMapper : CreatePackageMapper(serviceProvider);
        }
        /// <summary>
        /// 注册命令接口
        /// </summary>
        /// <param name="commandInterfaces">命令接口列表</param>
        /// <param name="commandSetFactories">命令设置工厂</param>
        /// <param name="serviceProvider">ServiceProvider</param>
        /// <param name="sessionType">Session类型</param>
        /// <param name="packageType">包类型</param>
        /// <param name="wrapRequired"></param>
        private void RegisterCommandInterfaces(List<CommandTypeInfo> commandInterfaces, List<ICommandSetFactory> commandSetFactories, IServiceProvider serviceProvider, Type sessionType, Type packageType, bool wrapRequired = false)
        {
            var genericTypes = new [] { sessionType, packageType };

            var commandInterface = typeof(ICommand<,>).GetTypeInfo().MakeGenericType(genericTypes);
            var asyncCommandInterface = typeof(IAsyncCommand<,>).GetTypeInfo().MakeGenericType(genericTypes);

            var commandSetFactoryType = typeof(CommandSetFactory<>).MakeGenericType(typeof(TKey), typeof(TNetPackageInfo), typeof(TPackageInfo), sessionType);

            var syncCommandType = new CommandTypeInfo(typeof(ICommand<,>).GetTypeInfo().MakeGenericType(genericTypes), commandSetFactoryType);
            var asyncCommandType = new CommandTypeInfo(typeof(IAsyncCommand<,>).GetTypeInfo().MakeGenericType(genericTypes), commandSetFactoryType);

            commandInterfaces.Add(syncCommandType);
            commandInterfaces.Add(asyncCommandType);

            if (wrapRequired)
            {
                syncCommandType.WrapRequired = true;
                syncCommandType.WrapFactory = (t) =>
                {
                    return typeof(CommandWrap<,,,>).GetTypeInfo().MakeGenericType(sessionType, typeof(TPackageInfo), packageType, t);
                };

                asyncCommandType.WrapRequired = true;
                asyncCommandType.WrapFactory = (t) =>
                {
                    return typeof(AsyncCommandWrap<,,,>).GetTypeInfo().MakeGenericType(sessionType, typeof(TPackageInfo), packageType, t);
                };
            }

            RegisterCommandSetFactoriesFromServices(commandSetFactories, serviceProvider, syncCommandType.CommandType, commandSetFactoryType, syncCommandType.WrapFactory);
            RegisterCommandSetFactoriesFromServices(commandSetFactories, serviceProvider, asyncCommandType.CommandType, commandSetFactoryType, asyncCommandType.WrapFactory);
        }
        /// <summary>
        /// 从服务注册命令设置工厂
        /// </summary>
        /// <param name="commandSetFactories">命令设置服务</param>
        /// <param name="serviceProvider">ServiceProvider</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandSetFactoryType">命令设置工厂类型</param>
        /// <param name="commandWrapFactory"></param>
        private void RegisterCommandSetFactoriesFromServices(List<ICommandSetFactory> commandSetFactories, IServiceProvider serviceProvider, Type commandType, Type commandSetFactoryType, Func<Type, Type> commandWrapFactory)
        {
            foreach (var command in serviceProvider.GetServices(commandType).OfType<ICommand>())
            {
                var cmd = command;
                var actualCommandType = cmd.GetType();

                if (commandWrapFactory != null)
                {
                    var commandWrapType = commandWrapFactory(command.GetType());
                    cmd = ActivatorUtilities.CreateInstance(null, commandWrapType, command) as ICommand;
                }

                var commandTypeInfo = new CommandTypeInfo(cmd);
                commandTypeInfo.ActualCommandType = actualCommandType;
                commandSetFactories.Add(ActivatorUtilities.CreateInstance(null, commandSetFactoryType, commandTypeInfo) as ICommandSetFactory);
            }
        }
        /// <summary>
        /// 创建数据包映射
        /// </summary>
        /// <param name="serviceProvider">ServiceProvider</param>
        /// <returns></returns>
        protected virtual IPackageMapper<TNetPackageInfo, TPackageInfo> CreatePackageMapper(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<IPackageMapper<TNetPackageInfo, TPackageInfo>>();
        }
        /// <summary>
        /// 句柄包
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="package">包</param>
        /// <returns></returns>
        protected virtual async ValueTask HandlePackage(IAppSession session, TPackageInfo package)
        {
            if (!_commands.TryGetValue(package.Key, out ICommandSet commandSet))
            {
                return;
            }

            await commandSet.ExecuteAsync(session, package);
        }
        /// <summary>
        /// 数据包接收后
        /// </summary>
        /// <param name="session"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        protected virtual async Task OnPackageReceived(IAppSession session, TPackageInfo package)
        {
            await HandlePackage(session, package);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        ValueTask IPackageHandler<TNetPackageInfo>.Handle(IAppSession session, TNetPackageInfo package)
        {
            return HandlePackage(session, PackageMapper.Map(package));
        }

        interface ICommandSet
        {
            TKey Key { get; }

            ValueTask ExecuteAsync(IAppSession session, TPackageInfo package);
        }

        class CommandTypeInfo
        {
            public Type CommandType { get; private set; }

            public Type ActualCommandType { get; set; }

            public ICommand Command { get; private set; }

            public Type CommandSetFactoryType { get; private set; }

            public bool WrapRequired { get; set; }

            public Func<Type, Type> WrapFactory { get; set; }
            /// <summary>
            /// 初始化命令类型信息
            /// </summary>
            /// <param name="command">命令</param>
            public CommandTypeInfo(ICommand command)
            {
                Command = command;
                CommandType = command.GetType();
            }
            /// <summary>
            /// 初始化命令类型信息
            /// </summary>
            /// <param name="commandType">命令类型</param>
            /// <param name="commandSetFactoryType">命令设置工厂类型</param>
            public CommandTypeInfo(Type commandType, Type commandSetFactoryType)
                : this(commandType, commandSetFactoryType, false)
            {

            }
            /// <summary>
            /// 初始化命令类型信息
            /// </summary>
            /// <param name="commandType">命令类型</param>
            /// <param name="commandSetFactoryType">命令设置工厂类型</param>
            /// <param name="wrapRequired"></param>
            public CommandTypeInfo(Type commandType, Type commandSetFactoryType, bool wrapRequired)
            {
                CommandType = commandType;
                CommandSetFactoryType = commandSetFactoryType;
                WrapRequired = wrapRequired;
            }
            /// <summary>
            /// 创建命令设置工厂
            /// </summary>
            /// <param name="type">类型</param>
            /// <returns></returns>
            public ICommandSetFactory CreateCommandSetFactory(Type type)
            {
                var commandTyeInfo = new CommandTypeInfo(WrapRequired ? WrapFactory(type) : type, null);
                commandTyeInfo.ActualCommandType = type;                
                return ActivatorUtilities.CreateInstance(null, this.CommandSetFactoryType, commandTyeInfo) as ICommandSetFactory;
            }
        }

        interface ICommandSetFactory
        {
            ICommandSet Create(IServiceProvider serviceProvider, CommandOptions commandOptions);
        }

        class CommandSetFactory<TAppSession> : ICommandSetFactory
            where TAppSession : IAppSession
        
        {
            public CommandTypeInfo CommandType { get; private set; }
            /// <summary>
            /// 命令设置工厂
            /// </summary>
            /// <param name="commandType">命令类型</param>
            public CommandSetFactory(CommandTypeInfo commandType)
            {
                CommandType = commandType;
            }
            /// <summary>
            /// 创建
            /// </summary>
            /// <param name="serviceProvider">ServiceProvider</param>
            /// <param name="commandOptions">命令选项</param>
            /// <returns></returns>
            public ICommandSet Create(IServiceProvider serviceProvider, CommandOptions commandOptions)
            {
                var commandSet = new CommandSet<TAppSession>();
                commandSet.Initialize(serviceProvider, CommandType, commandOptions);
                return commandSet;
            }
        }

        class CommandSet<TAppSession> : ICommandSet
            where TAppSession : IAppSession
        {
            public IAsyncCommand<TAppSession, TPackageInfo> AsyncCommand { get; private set; }

            public ICommand<TAppSession, TPackageInfo> Command { get; private set; }

            public IReadOnlyList<ICommandFilter> Filters { get; private set; }
            
            public CommandMetadata Metadata { get; private set; }

            public TKey Key { get; private set; }

            private readonly bool _isKeyString = false;
            /// <summary>
            /// 初始话命令设置
            /// </summary>
            public CommandSet()
            {
                _isKeyString = typeof(TKey) == typeof(string);
            }
            /// <summary>
            /// 获取命令元数据
            /// </summary>
            /// <param name="commandType">命令类型</param>
            /// <returns></returns>
            private CommandMetadata GetCommandMetadata(Type commandType)
            {
                var cmdAtt = commandType.GetCustomAttribute(typeof(CommandAttribute)) as CommandAttribute;
                var cmdMeta = default(CommandMetadata);

                if (cmdAtt == null)
                {
                    if (!_isKeyString)
                    {
                        throw new Exception($"The command {commandType.FullName} needs a CommandAttribute defined.");
                    }

                    cmdMeta = new CommandMetadata(commandType.Name, commandType.Name);
                }
                else
                {
                    var cmdName = cmdAtt.Name;

                    if (string.IsNullOrEmpty(cmdName))
                        cmdName = commandType.Name;

                    if (cmdAtt.Key == null)
                    {
                        if (!_isKeyString)
                        {
                            throw new Exception($"The command {commandType.FullName} needs a Key in type '{typeof(TKey).Name}' defined in its CommandAttribute.");
                        }

                        cmdMeta = new CommandMetadata(cmdName, cmdName);
                    }
                    else
                    {
                        cmdMeta = new CommandMetadata(cmdName, cmdAtt.Key);
                    }
                }

                return cmdMeta;
            }
            /// <summary>
            /// 设置命令
            /// </summary>
            /// <param name="command">命令</param>
            protected void SetCommand(ICommand command)
            {
                Command = command as ICommand<TAppSession, TPackageInfo>;
                AsyncCommand = command as IAsyncCommand<TAppSession, TPackageInfo>;
            }
            /// <summary>
            /// 初始化
            /// </summary>
            /// <param name="serviceProvider">ServiceProvider</param>
            /// <param name="commandTypeInfo">命令类型信息</param>
            /// <param name="commandOptions">命令选项</param>
            public void Initialize(IServiceProvider serviceProvider, CommandTypeInfo commandTypeInfo, CommandOptions commandOptions)
            {
                var command = commandTypeInfo.Command;

                if (command == null)
                {
                    if (commandTypeInfo.CommandType != commandTypeInfo.ActualCommandType)
                    {
                        var commandFactory = ActivatorUtilities.CreateFactory(commandTypeInfo.CommandType, new [] { typeof(IServiceProvider) });
                        command = commandFactory.Invoke(serviceProvider, new object[] { serviceProvider }) as ICommand;
                    }
                    else
                    {
                        command = ActivatorUtilities.CreateInstance(serviceProvider, commandTypeInfo.CommandType) as ICommand;
                    }                    
                }
                
                SetCommand(command);
                
                var cmdMeta = GetCommandMetadata(commandTypeInfo.ActualCommandType);

                try
                {
                    Key = (TKey)cmdMeta.Key;
                    Metadata = cmdMeta;
                }
                catch (Exception e)
                {
                    throw new Exception($"The command {cmdMeta.Name}'s Key {cmdMeta.Key} cannot be converted to the desired type '{typeof(TKey).Name}'.", e);
                }

                var filters = new List<ICommandFilter>();

                if (commandOptions.GlobalCommandFilterTypes.Any())
                    filters.AddRange(commandOptions.GlobalCommandFilterTypes.Select(t => ActivatorUtilities.CreateInstance(serviceProvider, t) as CommandFilterBaseAttribute));

                filters.AddRange(commandTypeInfo.ActualCommandType.GetCustomAttributes(false).OfType<CommandFilterBaseAttribute>());
                Filters = filters;
            }
            /// <summary>
            /// 异步执行
            /// </summary>
            /// <param name="session">Session</param>
            /// <param name="package">数据包</param>
            /// <returns></returns>
            public async ValueTask ExecuteAsync(IAppSession session, TPackageInfo package)
            {
                if (Filters.Count > 0)
                {
                    await ExecuteAsyncWithFilter(session, package);
                    return;
                }

                var appSession = (TAppSession)session;

                var asyncCommand = AsyncCommand;

                if (asyncCommand != null)
                {
                    await asyncCommand.ExecuteAsync(appSession, package);
                    return;
                }

                Command.Execute(appSession, package);
            }
            /// <summary>
            /// 异步执行筛选器
            /// </summary>
            /// <param name="session">Session</param>
            /// <param name="package">数据包</param>
            /// <returns></returns>
            private async ValueTask ExecuteAsyncWithFilter(IAppSession session, TPackageInfo package)
            {
                var context = new CommandExecutingContext();
                context.Package = package;
                context.Session = session;

                var command = AsyncCommand != null ? (AsyncCommand as ICommand) : (Command as ICommand);

                if (command is ICommandWrap commandWrap)
                    command = commandWrap.InnerCommand;

                context.CurrentCommand = command;

                var filters = Filters;

                var continued = true;

                for (var i = 0; i < filters.Count; i++)
                {
                    var f = filters[i];
                    
                    if (f is AsyncCommandFilterAttribute asyncCommandFilter)
                    {
                        continued = await asyncCommandFilter.OnCommandExecutingAsync(context);
                    }
                    else if (f is CommandFilterAttribute commandFilter)
                    {
                        continued = commandFilter.OnCommandExecuting(context);
                    }

                    if (!continued)
                        break;
                }

                if (!continued)
                    return;                

                try
                {
                    var appSession = (TAppSession)session;
                    var asyncCommand = AsyncCommand;

                    if (asyncCommand != null)
                    {
                        await asyncCommand.ExecuteAsync(appSession, package);
                    }
                    else
                    {
                        Command.Execute(appSession, package);
                    }                    
                }
                catch (Exception e)
                {
                    context.Exception = e;
                }
                finally
                {
                    for (var i = 0; i < filters.Count; i++)
                    {
                        var f = filters[i];
                        
                        if (f is AsyncCommandFilterAttribute asyncCommandFilter)
                        {
                            await asyncCommandFilter.OnCommandExecutedAsync(context);
                        }
                        else if (f is CommandFilterAttribute commandFilter)
                        {
                            commandFilter.OnCommandExecuted(context);
                        }
                    }
                }
            }
            /// <summary>
            /// 将命令的类型转为字符串
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                ICommand command = Command;
                
                if (command == null)
                    command = AsyncCommand;

                return command?.GetType().ToString();
            }
        }
    }
}
