using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using SuperSocket.Channel;

namespace SuperSocket
{
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// 作为SuperSocket Host Builder
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <param name="hostBuilder">Host Builder</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<TReceivePackage> AsSuperSocketHostBuilder<TReceivePackage>(this IHostBuilder hostBuilder)
        {
            if (hostBuilder is ISuperSocketHostBuilder<TReceivePackage> ssHostBuilder)
            {
                return ssHostBuilder;
            }

            return new SuperSocketHostBuilder<TReceivePackage>(hostBuilder);
        }
        /// <summary>
        /// 作为SuperSocket Host Builder
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <typeparam name="TPipelineFilter">管道筛选器</typeparam>
        /// <param name="hostBuilder">Host Builder</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<TReceivePackage> AsSuperSocketHostBuilder<TReceivePackage, TPipelineFilter>(this IHostBuilder hostBuilder)
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            if (hostBuilder is ISuperSocketHostBuilder<TReceivePackage> ssHostBuilder)
            {
                return ssHostBuilder;
            }

            return (new SuperSocketHostBuilder<TReceivePackage>(hostBuilder))
                .UsePipelineFilter<TPipelineFilter>();
        }
        /// <summary>
        /// 使用管道筛选器工厂
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <param name="hostBuilder">Host Builder</param>
        /// <param name="filterFactory">管道筛选器</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<object, IPipelineFilter<TReceivePackage>> filterFactory)
        {
            hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<object, IPipelineFilter<TReceivePackage>>>(filterFactory);
                }
            );

            return hostBuilder.UsePipelineFilterFactory<DelegatePipelineFilterFactory<TReceivePackage>>();
        }
        /// <summary>
        /// 使用清除空闲Session
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <param name="hostBuilder">Host Builder</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<TReceivePackage> UseClearIdleSession<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
        {
            return hostBuilder.UseMiddleware<ClearIdleSessionMiddleware>();
        }
        /// <summary>
        /// 使用Session处理
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <param name="hostBuilder">Host Builder</param>
        /// <param name="onConnected">连接后</param>
        /// <param name="onClosed">关闭后</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<TReceivePackage> UseSessionHandler<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, ValueTask> onConnected = null, Func<IAppSession, CloseEventArgs, ValueTask> onClosed = null)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<SessionHandlers>(new SessionHandlers
                    {
                        Connected = onConnected,
                        Closed = onClosed
                    });
                }
            );
        }        
        /// <summary>
        /// 配置SuperSocket
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <param name="hostBuilder">Host Buider</param>
        /// <param name="configurator">配置器</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<TReceivePackage> ConfigureSuperSocket<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Action<ServerOptions> configurator)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.Configure<ServerOptions>(configurator);
                }
            );
        }
        /// <summary>
        /// 配置Socket选项
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <param name="hostBuilder">Host Builder</param>
        /// <param name="socketOptionsSetter">Socket选项设置器</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<TReceivePackage> ConfigureSocketOptions<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Action<Socket> socketOptionsSetter)
            where TReceivePackage : class
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<SocketOptionsSetter>(new SocketOptionsSetter(socketOptionsSetter));
                }
            );
        }
        /// <summary>
        /// 建立服务器
        /// </summary>
        /// <param name="hostBuilder">Host Builder</param>
        /// <returns></returns>
        public static IServer BuildAsServer(this IHostBuilder hostBuilder)
        {
            var host = hostBuilder.Build();
            return host.AsServer();
        }
        /// <summary>
        /// 作为服务器
        /// </summary>
        /// <param name="host">主机</param>
        /// <returns></returns>
        public static IServer AsServer(this IHost host)
        {
            return host.Services.GetService<IEnumerable<IHostedService>>().OfType<IServer>().FirstOrDefault();
        }
        /// <summary>
        /// 配置错误处理
        /// </summary>
        /// <typeparam name="TReceivePackage">接受包的类型</typeparam>
        /// <param name="hostBuilder">Host Builder</param>
        /// <param name="errorHandler">错误处理</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<TReceivePackage> ConfigureErrorHandler<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>>>(errorHandler);
                }
            );
        }
        /// <summary>
        /// 使用包处理
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <param name="hostBuilder">Host Builder</param>
        /// <param name="packageHandler">包处理</param>
        /// <param name="errorHandler">错误处理</param>
        /// <returns></returns>
        // move to extensions
        public static ISuperSocketHostBuilder<TReceivePackage> UsePackageHandler<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, TReceivePackage, ValueTask> packageHandler, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler = null)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    if (packageHandler != null)
                        services.AddSingleton<IPackageHandler<TReceivePackage>>(new DelegatePackageHandler<TReceivePackage>(packageHandler));

                    if (errorHandler != null)
                        services.AddSingleton<Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>>>(errorHandler);
                }
            );
        }
        /// <summary>
        /// 作为多服务器主机构建
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static MultipleServerHostBuilder AsMultipleServerHostBuilder(this IHostBuilder hostBuilder)
        {
            return new MultipleServerHostBuilder(hostBuilder);
        }
    }
}
