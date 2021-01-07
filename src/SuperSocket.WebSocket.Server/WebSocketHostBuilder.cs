using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket.Server;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.Server
{
    class WebSocketHostBuilderAdapter : ServerHostBuilderAdapter<WebSocketPackage>
    {
        /// <summary>
        /// 初始化WebSocket主机构建适配器
        /// </summary>
        /// <param name="hostBuilder">Host Builder</param>
        public WebSocketHostBuilderAdapter(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {
            this.UsePipelineFilter<WebSocketPipelineFilter>();
            this.UseWebSocketMiddleware();
            this.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IPackageHandler<WebSocketPackage>, WebSocketPackageHandler>();
            });
            this.ConfigureSupplementServices(WebSocketHostBuilder.ValidateHostBuilder);
        }
        /// <summary>
        /// 注册默认服务
        /// </summary>
        /// <param name="builderContext">构建器上下文</param>
        /// <param name="servicesInHost">主机服务</param>
        /// <param name="services">服务</param>
        protected override void RegisterDefaultServices(HostBuilderContext builderContext, IServiceCollection servicesInHost, IServiceCollection services)
        {
            services.TryAddSingleton<ISessionFactory, GenericSessionFactory<WebSocketSession>>();
        }
    }

    public class WebSocketHostBuilder : SuperSocketHostBuilder<WebSocketPackage>
    {
        /// <summary>
        /// 初始化WebSocket主机构建器
        /// </summary>
        internal WebSocketHostBuilder()
            : this(args: null)
        {

        }
        /// <summary>
        /// 初始化WebSocket主机构建器
        /// </summary>
        /// <param name="hostBuilder">Host Builder</param>
        internal WebSocketHostBuilder(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {
            
        }
        /// <summary>
        /// 初始化WebSocket主机构建器
        /// </summary>
        /// <param name="args">参数</param>
        internal WebSocketHostBuilder(string[] args)
            : base(args)
        {
            this.ConfigureSupplementServices(WebSocketHostBuilder.ValidateHostBuilder);
        }
        /// <summary>
        /// 注册默认服务
        /// </summary>
        /// <param name="builderContext">构建器上下文</param>
        /// <param name="servicesInHost">主机服务</param>
        /// <param name="services">服务</param>
        protected override void RegisterDefaultServices(HostBuilderContext builderContext, IServiceCollection servicesInHost, IServiceCollection services)
        {
            base.RegisterDefaultServices(builderContext, servicesInHost, services);
            services.TryAddSingleton<ISessionFactory, GenericSessionFactory<WebSocketSession>>();
        }        
        /// <summary>
        /// 创建
        /// </summary>
        /// <returns></returns>
        public static WebSocketHostBuilder Create()
        {
            return Create(args: null);
        }
        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static WebSocketHostBuilder Create(string[] args)
        {
            return Create(new WebSocketHostBuilder(args));
        }
        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="hostBuilder">Host Builder</param>
        /// <returns></returns>
        public static WebSocketHostBuilder Create(SuperSocketHostBuilder<WebSocketPackage> hostBuilder)
        {
            return hostBuilder.UsePipelineFilter<WebSocketPipelineFilter>()
                .UseWebSocketMiddleware()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<IPackageHandler<WebSocketPackage>, WebSocketPackageHandler>();
                }) as WebSocketHostBuilder;
        }
        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="hostBuilder">Host Builder</param>
        /// <returns></returns>
        public static WebSocketHostBuilder Create(IHostBuilder hostBuilder)
        {
            return Create(new WebSocketHostBuilder(hostBuilder));
        }
        /// <summary>
        /// 验证主机构建器
        /// </summary>
        /// <param name="builderCtx">构建器上下文</param>
        /// <param name="services">服务</param>
        internal static void ValidateHostBuilder(HostBuilderContext builderCtx, IServiceCollection services)
        {
            
        }
    }
}
