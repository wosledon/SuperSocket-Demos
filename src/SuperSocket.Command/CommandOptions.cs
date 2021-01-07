using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperSocket.Command
{
    public class CommandOptions : ICommandSource
    {
        /// <summary>
        /// 初始化命令设置
        /// </summary>
        public CommandOptions()
        {
            CommandSources = new List<ICommandSource>();
            _globalCommandFilterTypes = new List<Type>();
        }

        public CommandAssemblyConfig[] Assemblies { get; set; }

        public List<ICommandSource> CommandSources { get; set; }

        private List<Type> _globalCommandFilterTypes;

        public IReadOnlyList<Type> GlobalCommandFilterTypes
        {
            get { return _globalCommandFilterTypes; }
        }
        /// <summary>
        /// 获取命令类型
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria)
        {
            var commandSources = CommandSources;
            var configuredAssemblies = Assemblies;

            if (configuredAssemblies != null && configuredAssemblies.Any())
            {
                commandSources.AddRange(configuredAssemblies);
            }

            var commandTypes = new List<Type>();

            foreach (var source in commandSources)
            {
                commandTypes.AddRange(source.GetCommandTypes(criteria));
            }

            return commandTypes;
        }
        /// <summary>
        /// 添加全局命令筛选器类型
        /// </summary>
        /// <param name="commandFilterType"></param>
        internal void AddGlobalCommandFilterType(Type commandFilterType)
        {
            _globalCommandFilterTypes.Add(commandFilterType);
        }
    }
    /// <summary>
    /// 命令装配配置
    /// </summary>
    public class CommandAssemblyConfig : AssemblyBaseCommandSource, ICommandSource
    {
        public string Name { get; set; }
        /// <summary>
        /// 去获取命令类型
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria)
        {
            return GetCommandTypesFromAssembly(Assembly.Load(Name)).Where(t => criteria(t));
        }
    }
    /// <summary>
    /// 当前命令装配
    /// </summary>
    public class ActualCommandAssembly : AssemblyBaseCommandSource, ICommandSource
    {
        public Assembly Assembly { get; set; }
        /// <summary>
        /// 获取命令类型
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria)
        {
            return GetCommandTypesFromAssembly(Assembly).Where(t => criteria(t));
        }
    }
    /// <summary>
    /// 装配基命令源
    /// </summary>
    public abstract class AssemblyBaseCommandSource
    {
        public IEnumerable<Type> GetCommandTypesFromAssembly(Assembly assembly)
        {
            return assembly.GetExportedTypes();
        }
    }
    /// <summary>
    /// 当前命令
    /// </summary>
    public class ActualCommand : ICommandSource
    {
        public Type CommandType { get; set; }

        public IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria)
        {
            if (criteria(CommandType))
                yield return CommandType;
        }
    }
    /// <summary>
    /// 命令选项扩展
    /// </summary>
    public static class CommandOptionsExtensions
    {
        /// <summary>
        /// 添加命令
        /// </summary>
        /// <typeparam name="TCommand">命令</typeparam>
        /// <param name="commandOptions">命令选项</param>
        public static void AddCommand<TCommand>(this CommandOptions commandOptions)
        {
            commandOptions.CommandSources.Add(new ActualCommand { CommandType = typeof(TCommand) });
        }
        /// <summary>
        /// 添加命令
        /// </summary>
        /// <param name="commandOptions">命令选项</param>
        /// <param name="commandType">命令类型</param>
        public static void AddCommand(this CommandOptions commandOptions, Type commandType)
        {
            commandOptions.CommandSources.Add(new ActualCommand { CommandType = commandType });
        }
        /// <summary>
        /// 添加命令装配
        /// </summary>
        /// <param name="commandOptions">命令选项</param>
        /// <param name="commandAssembly">命令装配</param>
        public static void AddCommandAssembly(this CommandOptions commandOptions, Assembly commandAssembly)
        {
            commandOptions.CommandSources.Add(new ActualCommandAssembly { Assembly = commandAssembly });
        }
        /// <summary>
        /// 添加全局命令筛选器
        /// </summary>
        /// <typeparam name="TCommandFilter">命令筛选器</typeparam>
        /// <param name="commandOptions">命令选项</param>
        public static void AddGlobalCommandFilter<TCommandFilter>(this CommandOptions commandOptions)
            where TCommandFilter : CommandFilterBaseAttribute
        {
            commandOptions.AddGlobalCommandFilterType(typeof(TCommandFilter));
        }
        /// <summary>
        /// 添加全局命令筛选器
        /// </summary>
        /// <param name="commandOptions">命令选项</param>
        /// <param name="commandFilterType">命令筛选器类型</param>
        public static void AddGlobalCommandFilter(this CommandOptions commandOptions, Type commandFilterType)
        {
            if (!typeof(CommandFilterBaseAttribute).IsAssignableFrom(commandFilterType))
                throw new Exception("The command filter type must inherit CommandFilterBaseAttribute.");

            commandOptions.AddGlobalCommandFilterType(commandFilterType);
        }
    }
}
