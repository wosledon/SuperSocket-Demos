using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket
{
    public class SuperSocketHostBuilder<TReceivePackage> : HostBuilderAdapter<SuperSocketHostBuilder<TReceivePackage>>, ISuperSocketHostBuilder<TReceivePackage>, IHostBuilder
    {
        private Func<HostBuilderContext, IConfiguration, IConfiguration> _serverOptionsReader;

        protected List<Action<HostBuilderContext, IServiceCollection>> ConfigureServicesActions { get; private set; } = new List<Action<HostBuilderContext, IServiceCollection>>();

        protected List<Action<HostBuilderContext, IServiceCollection>> ConfigureSupplementServicesActions = new List<Action<HostBuilderContext, IServiceCollection>>();
        /// <summary>
        /// 初始化主机构建器
        /// </summary>
        /// <param name="hostBuilder"></param>
        public SuperSocketHostBuilder(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {

        }
        /// <summary>
        /// 初始化主机构建器
        /// </summary>
        public SuperSocketHostBuilder()
            : this(args: null)
        {

        }
        /// <summary>
        /// 初始化主机构建器
        /// </summary>
        /// <param name="args">参数</param>
        public SuperSocketHostBuilder(string[] args)
            : base(args)
        {

        }
        /// <summary>
        /// 构建
        /// </summary>
        /// <returns></returns>
        public override IHost Build()
        {
            return HostBuilder.ConfigureServices((ctx, services) =>
            {
                RegisterBasicServices(ctx, services, services);
            }).ConfigureServices((ctx, services) =>
            {
                foreach (var action in ConfigureServicesActions)
                {
                    action(ctx, services);
                }

                foreach (var action in ConfigureSupplementServicesActions)
                {
                    action(ctx, services);
                }
            }).ConfigureServices((ctx, services) =>
            {
                RegisterDefaultServices(ctx, services, services);
            }).Build();
        }
        /// <summary>
        /// 配置补充服务
        /// </summary>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        public ISuperSocketHostBuilder<TReceivePackage> ConfigureSupplementServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            ConfigureSupplementServicesActions.Add(configureDelegate);
            return this;
        }
        /// <summary>
        /// 配置补充服务
        /// </summary>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        ISuperSocketHostBuilder ISuperSocketHostBuilder.ConfigureSupplementServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            return ConfigureSupplementServices(configureDelegate);
        }
        /// <summary>
        /// 注册基本服务
        /// </summary>
        /// <param name="builderContext">构造器上下文</param>
        /// <param name="servicesInHost">主机服务</param>
        /// <param name="services">服务</param>
        protected virtual void RegisterBasicServices(HostBuilderContext builderContext, IServiceCollection servicesInHost, IServiceCollection services)
        {
            var serverOptionReader = _serverOptionsReader;

            if (serverOptionReader == null)
            {
                serverOptionReader = (ctx, config) =>
                {
                    return config;
                };
            }

            services.AddOptions();

            var config = builderContext.Configuration.GetSection("serverOptions");
            var serverConfig = serverOptionReader(builderContext, config);

            services.Configure<ServerOptions>(serverConfig);
        }
        /// <summary>
        /// 注册默认服务
        /// </summary>
        /// <param name="builderContext">构建器上下文</param>
        /// <param name="servicesInHost">主机服务</param>
        /// <param name="services">服务</param>
        protected virtual void RegisterDefaultServices(HostBuilderContext builderContext, IServiceCollection servicesInHost, IServiceCollection services)
        {
            // if the package type is StringPackageInfo
            if (typeof(TReceivePackage) == typeof(StringPackageInfo))
            {
                services.TryAdd(ServiceDescriptor.Singleton<IPackageDecoder<StringPackageInfo>, DefaultStringPackageDecoder>());
            }

            services.TryAdd(ServiceDescriptor.Singleton<IPackageEncoder<string>, DefaultStringEncoderForDI>());

            // if no host service was defined, just use the default one
            if (!CheckIfExistHostedService(services))
            {
                RegisterDefaultHostedService(servicesInHost);
            }
        }
        /// <summary>
        /// 检查是否存在主机服务
        /// </summary>
        /// <param name="services">服务</param>
        /// <returns></returns>
        protected virtual bool CheckIfExistHostedService(IServiceCollection services)
        {
            return services.Any(s => s.ServiceType == typeof(IHostedService)
                && typeof(SuperSocketService<TReceivePackage>).IsAssignableFrom(GetImplementationType(s)));
        }
        /// <summary>
        /// 获取实现类型
        /// </summary>
        /// <param name="serviceDescriptor">服务描述符</param>
        /// <returns></returns>
        private Type GetImplementationType(ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.ImplementationType != null)
                return serviceDescriptor.ImplementationType;

            if (serviceDescriptor.ImplementationInstance != null)
                return serviceDescriptor.ImplementationInstance.GetType();

            if (serviceDescriptor.ImplementationFactory != null)
            {
                var typeArguments = serviceDescriptor.ImplementationFactory.GetType().GenericTypeArguments;

                if (typeArguments.Length == 2)
                    return typeArguments[1];
            }

            return null;
        }
        /// <summary>
        /// 注册默认主机服务
        /// </summary>
        /// <param name="servicesInHost">主机服务</param>
        protected virtual void RegisterDefaultHostedService(IServiceCollection servicesInHost)
        {
            RegisterHostedService<SuperSocketService<TReceivePackage>>(servicesInHost);
        }
        /// <summary>
        /// 注册主机服务
        /// </summary>
        /// <typeparam name="THostedService">主机服务的类型</typeparam>
        /// <param name="servicesInHost">主机服务</param>
        protected virtual void RegisterHostedService<THostedService>(IServiceCollection servicesInHost)
            where THostedService : class, IHostedService
        {
            servicesInHost.AddSingleton<THostedService, THostedService>();
            servicesInHost.AddSingleton<IServerInfo>(s => s.GetService<THostedService>() as IServerInfo);
            servicesInHost.AddHostedService<THostedService>(s => s.GetService<THostedService>());
        }
        /// <summary>
        /// 配置服务器设置
        /// </summary>
        /// <param name="serverOptionsReader"></param>
        /// <returns></returns>
        public ISuperSocketHostBuilder<TReceivePackage> ConfigureServerOptions(Func<HostBuilderContext, IConfiguration, IConfiguration> serverOptionsReader)
        {
            _serverOptionsReader = serverOptionsReader;
            return this;
        }
        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        ISuperSocketHostBuilder<TReceivePackage> ISuperSocketHostBuilder<TReceivePackage>.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            return ConfigureServices(configureDelegate);
        }
        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        public override SuperSocketHostBuilder<TReceivePackage> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            ConfigureServicesActions.Add(configureDelegate);
            return this;
        }
        /// <summary>
        /// 只用管道筛选器
        /// </summary>
        /// <typeparam name="TPipelineFilter">管道筛选器的类型</typeparam>
        /// <returns></returns>
        public virtual ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilter<TPipelineFilter>()
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return this.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IPipelineFilterFactory<TReceivePackage>, DefaultPipelineFilterFactory<TReceivePackage, TPipelineFilter>>();
            });
        }
        /// <summary>
        /// 使用管道筛选器工厂
        /// </summary>
        /// <typeparam name="TPipelineFilterFactory">管道筛选器工厂类型</typeparam>
        /// <returns></returns>
        public virtual ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TPipelineFilterFactory>()
            where TPipelineFilterFactory : class, IPipelineFilterFactory<TReceivePackage>
        {
            return this.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IPipelineFilterFactory<TReceivePackage>, TPipelineFilterFactory>();
            });
        }
        /// <summary>
        /// 使用Session
        /// </summary>
        /// <typeparam name="TSession"></typeparam>
        /// <returns></returns>
        public virtual ISuperSocketHostBuilder<TReceivePackage> UseSession<TSession>()
            where TSession : IAppSession
        {
            return this.UseSessionFactory<GenericSessionFactory<TSession>>();
        }
        /// <summary>
        /// 使用Session工厂
        /// </summary>
        /// <typeparam name="TSessionFactory"></typeparam>
        /// <returns></returns>
        public virtual ISuperSocketHostBuilder<TReceivePackage> UseSessionFactory<TSessionFactory>()
            where TSessionFactory : class, ISessionFactory
        {
            return this.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<ISessionFactory, TSessionFactory>();
                }
            );
        }
        /// <summary>
        /// 使用主机服务
        /// </summary>
        /// <typeparam name="THostedService"></typeparam>
        /// <returns></returns>
        public virtual ISuperSocketHostBuilder<TReceivePackage> UseHostedService<THostedService>()
            where THostedService : class, IHostedService
        {
            if (!typeof(SuperSocketService<TReceivePackage>).IsAssignableFrom(typeof(THostedService)))
            {
                throw new ArgumentException($"The type parameter should be subclass of {nameof(SuperSocketService<TReceivePackage>)}", nameof(THostedService));
            }

            return this.ConfigureServices((ctx, services) =>
            {
                RegisterHostedService<THostedService>(services);
            });
        }

        /// <summary>
        /// 使用包解码器
        /// </summary>
        /// <typeparam name="TPackageDecoder">包解码器的类型</typeparam>
        /// <returns></returns>
        public virtual ISuperSocketHostBuilder<TReceivePackage> UsePackageDecoder<TPackageDecoder>()
            where TPackageDecoder : class, IPackageDecoder<TReceivePackage>
        {
            return this.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<IPackageDecoder<TReceivePackage>, TPackageDecoder>();
                }
            );
        }
        /// <summary>
        /// 使用中间件
        /// </summary>
        /// <typeparam name="TMiddleware">中间件的类型</typeparam>
        /// <returns></returns>
        public virtual ISuperSocketHostBuilder<TReceivePackage> UseMiddleware<TMiddleware>()
            where TMiddleware : class, IMiddleware
        {
            return this.ConfigureServices((ctx, services) =>
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, TMiddleware>());
            });
        }
        /// <summary>
        /// 使用包处理调度器
        /// </summary>
        /// <typeparam name="TPackageHandlingScheduler">包处理调度器的类型</typeparam>
        /// <returns></returns>
        public ISuperSocketHostBuilder<TReceivePackage> UsePackageHandlingScheduler<TPackageHandlingScheduler>()
            where TPackageHandlingScheduler : class, IPackageHandlingScheduler<TReceivePackage>
        {
            return this.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<IPackageHandlingScheduler<TReceivePackage>, TPackageHandlingScheduler>();
                }
            );
        }
        /// <summary>
        /// 使用包处理上下文访问器
        /// </summary>
        /// <returns></returns>
        public ISuperSocketHostBuilder<TReceivePackage> UsePackageHandlingContextAccessor()
        {
            return this.ConfigureServices(
                 (hostCtx, services) =>
                 {
                     services.AddSingleton<IPackageHandlingContextAccessor<TReceivePackage>, PackageHandlingContextAccessor<TReceivePackage>>();
                 }
             );
        }
    }

    public static class SuperSocketHostBuilder
    {
        /// <summary>
        /// 创建SuperSocket主机构建器
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage>()
            where TReceivePackage : class
        {
            return Create<TReceivePackage>(args: null);
        }
        /// <summary>
        /// 创建SuperSocket主机构建器
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage>(string[] args)
        {
            return new SuperSocketHostBuilder<TReceivePackage>(args);
        }
        /// <summary>
        /// 创建SuperSocket主机构建器
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <typeparam name="TPipelineFilter">管道筛选器</typeparam>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage, TPipelineFilter>()
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return Create<TReceivePackage, TPipelineFilter>(args: null);
        }
        /// <summary>
        /// 创建SuperSocket主机构建器
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <typeparam name="TPipelineFilter">管道筛选器</typeparam>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static ISuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage, TPipelineFilter>(string[] args)
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return new SuperSocketHostBuilder<TReceivePackage>(args)
                .UsePipelineFilter<TPipelineFilter>();
        }
    }
}
