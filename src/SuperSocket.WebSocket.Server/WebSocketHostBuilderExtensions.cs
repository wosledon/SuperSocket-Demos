using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SuperSocket.Command;
using SuperSocket.Server;
using SuperSocket.WebSocket.Server.Extensions;
using SuperSocket.WebSocket.Server.Extensions.Compression;

namespace SuperSocket.WebSocket.Server
{
    public static class WebSocketServerExtensions
    {
        /// <summary>
        /// 使用WebSocket中间件
        /// </summary>
        /// <param name="builder">构建器</param>
        /// <returns></returns>
        internal static ISuperSocketHostBuilder<WebSocketPackage> UseWebSocketMiddleware(this ISuperSocketHostBuilder<WebSocketPackage> builder)
        {
            return builder
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<IWebSocketServerMiddleware, WebSocketServerMiddleware>();
                })
                .UseMiddleware<WebSocketServerMiddleware>(s => s.GetService<IWebSocketServerMiddleware>() as WebSocketServerMiddleware)
                as ISuperSocketHostBuilder<WebSocketPackage>;
        }
        /// <summary>
        /// 使用WebSocket消息处理器
        /// </summary>
        /// <param name="builder">构建器</param>
        /// <param name="handler">处理器</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<WebSocketPackage> UseWebSocketMessageHandler(this ISuperSocketHostBuilder<WebSocketPackage> builder, Func<WebSocketSession, WebSocketPackage, ValueTask> handler)
        {
            return builder.ConfigureServices((ctx, services) => 
            {
                services.AddSingleton<Func<WebSocketSession, WebSocketPackage, ValueTask>>(handler);
            }) as ISuperSocketHostBuilder<WebSocketPackage>;
        }
        /// <summary>
        /// 使用WebSocket消息处理器
        /// </summary>
        /// <param name="builder">构建器</param>
        /// <param name="protocol">协议</param>
        /// <param name="handler">处理器</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<WebSocketPackage> UseWebSocketMessageHandler(this ISuperSocketHostBuilder<WebSocketPackage> builder, string protocol, Func<WebSocketSession, WebSocketPackage, ValueTask> handler)
        {
            return builder.ConfigureServices((ctx, services) => 
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(ISubProtocolHandler), new DelegateSubProtocolHandler(protocol, handler)));
            }) as ISuperSocketHostBuilder<WebSocketPackage>;
        }
        /// <summary>
        /// 使用命令
        /// </summary>
        /// <typeparam name="TPackageInfo">包信息的类型</typeparam>
        /// <typeparam name="TPackageMapper">包映射的类型</typeparam>
        /// <param name="builder">构建器</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<WebSocketPackage> UseCommand<TPackageInfo, TPackageMapper>(this ISuperSocketHostBuilder<WebSocketPackage> builder)
            where TPackageInfo : class
            where TPackageMapper : class, IPackageMapper<WebSocketPackage, TPackageInfo>
        {
            var keyType = CommandMiddlewareExtensions.GetKeyType<TPackageInfo>();
            var commandMiddlewareType = typeof(WebSocketCommandMiddleware<,>).MakeGenericType(keyType, typeof(TPackageInfo));
            
            return builder.ConfigureServices((ctx, services) => 
            {
                services.AddSingleton(typeof(IWebSocketCommandMiddleware), commandMiddlewareType);
                services.AddSingleton<IPackageMapper<WebSocketPackage, TPackageInfo>, TPackageMapper>();
            }).ConfigureServices((ctx, services) =>
            {
                services.Configure<CommandOptions>(ctx.Configuration?.GetSection("serverOptions")?.GetSection("commands"));
            }) as ISuperSocketHostBuilder<WebSocketPackage>;
        }
        /// <summary>
        /// 使用命令
        /// </summary>
        /// <typeparam name="TPackageInfo">包信息的类型</typeparam>
        /// <typeparam name="TPackageMapper">包映射的类型</typeparam>
        /// <param name="builder">构建器</param>
        /// <param name="configurator">配置器</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<WebSocketPackage> UseCommand<TPackageInfo, TPackageMapper>(this ISuperSocketHostBuilder<WebSocketPackage> builder, Action<CommandOptions> configurator)
            where TPackageInfo : class
            where TPackageMapper : class, IPackageMapper<WebSocketPackage, TPackageInfo>, new()
        {
             return builder.UseCommand<TPackageInfo, TPackageMapper>()
                .ConfigureServices((ctx, services) =>
                {
                    services.Configure(configurator);
                }) as ISuperSocketHostBuilder<WebSocketPackage>;
        }
        /// <summary>
        /// 使用命令
        /// </summary>
        /// <typeparam name="TPackageInfo">包信息的类型</typeparam>
        /// <typeparam name="TPackageMapper">包映射的类型</typeparam>
        /// <param name="builder">构建器</param>
        /// <param name="protocol">协议</param>
        /// <param name="commandOptionsAction">命令设置活动</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<WebSocketPackage> UseCommand<TPackageInfo, TPackageMapper>(this ISuperSocketHostBuilder<WebSocketPackage> builder, string protocol, Action<CommandOptions> commandOptionsAction = null)
            where TPackageInfo : class
            where TPackageMapper : class, IPackageMapper<WebSocketPackage, TPackageInfo>
        {
            
            return builder.ConfigureServices((ctx, services) => 
            {                
                var commandOptions = new CommandOptions();                
                ctx.Configuration?.GetSection("serverOptions")?.GetSection("commands")?.GetSection(protocol)?.Bind(commandOptions);                
                commandOptionsAction?.Invoke(commandOptions);
                var commandOptionsWrapper = new OptionsWrapper<CommandOptions>(commandOptions);

                services.TryAddEnumerable(ServiceDescriptor.Singleton<ISubProtocolHandler, CommandSubProtocolHandler<TPackageInfo>>((sp) =>
                {
                    var mapper = ActivatorUtilities.CreateInstance<TPackageMapper>(sp);
                    return new CommandSubProtocolHandler<TPackageInfo>(protocol, sp, commandOptionsWrapper, mapper);
                }));
            }) as ISuperSocketHostBuilder<WebSocketPackage>;
        }
        /// <summary>
        /// 使用消息压缩
        /// </summary>
        /// <param name="builder">构建器</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<WebSocketPackage> UsePerMessageCompression(this ISuperSocketHostBuilder<WebSocketPackage> builder)
        {
             return builder.ConfigureServices((ctx, services) =>
             {
                 services.TryAddEnumerable(ServiceDescriptor.Singleton<IWebSocketExtensionFactory, WebSocketPerMessageCompressionExtensionFactory>());
             });
        }
        /// <summary>
        /// 添加WebSocket服务器
        /// </summary>
        /// <param name="hostBuilder">主机构建器</param>
        /// <param name="hostBuilderDelegate">主机构建器委托</param>
        /// <returns></returns>
        public static MultipleServerHostBuilder AddWebSocketServer(this MultipleServerHostBuilder hostBuilder, Action<ISuperSocketHostBuilder<WebSocketPackage>> hostBuilderDelegate)
        {
            return hostBuilder.AddWebSocketServer<SuperSocketService<WebSocketPackage>>(hostBuilderDelegate);
        }
        /// <summary>
        /// 添加WebSocket服务器
        /// </summary>
        /// <typeparam name="TWebSocketService">WebSocket服务类型</typeparam>
        /// <param name="hostBuilder">主机构建器</param>
        /// <param name="hostBuilderDelegate">主机构建器委托</param>
        /// <returns></returns>
        public static MultipleServerHostBuilder AddWebSocketServer<TWebSocketService>(this MultipleServerHostBuilder hostBuilder, Action<ISuperSocketHostBuilder<WebSocketPackage>> hostBuilderDelegate)
            where TWebSocketService : SuperSocketService<WebSocketPackage>
        {
            var appHostBuilder = new WebSocketHostBuilderAdapter(hostBuilder);

            appHostBuilder
                .UseHostedService<TWebSocketService>();

            hostBuilderDelegate?.Invoke(appHostBuilder);

            hostBuilder.AddServer(appHostBuilder);
            return hostBuilder;
        }
        /// <summary>
        /// 作为WebSocket主机构建器
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static WebSocketHostBuilder AsWebSocketHostBuilder(this IHostBuilder hostBuilder)
        {
            return WebSocketHostBuilder.Create(hostBuilder);
        }
    }
}
