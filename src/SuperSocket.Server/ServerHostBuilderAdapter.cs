using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SuperSocket.Server
{
    public interface IServerHostBuilderAdapter
    {
        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="context">Host Builder上下文</param>
        /// <param name="hostServices"></param>
        void ConfigureServer(HostBuilderContext context, IServiceCollection hostServices);
        /// <summary>
        /// 配置服务供应
        /// </summary>
        /// <param name="hostServiceProvider">Service Provider</param>
        void ConfigureServiceProvider(IServiceProvider hostServiceProvider);
    }

    public class ServerHostBuilderAdapter<TReceivePackage> : SuperSocketHostBuilder<TReceivePackage>, IServerHostBuilderAdapter
    {
        private IHostBuilder _hostBuilder;

        private IServiceCollection _currentServices = new ServiceCollection();
        
        private IServiceProvider _serviceProvider;

        private IServiceProvider _hostServiceProvider;

        private Func<HostBuilderContext, IServiceCollection, IServiceProvider> _serviceProviderBuilder = null;

        private List<IConfigureContainerAdapter> _configureContainerActions = new List<IConfigureContainerAdapter>();
        /// <summary>
        /// 服务器主机构建适配器
        /// </summary>
        /// <param name="hostBuilder"></param>
        public ServerHostBuilderAdapter(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {
            _hostBuilder = hostBuilder;
        }
        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="context">Host Builder上下文</param>
        /// <param name="hostServices">主机服务</param>
        void IServerHostBuilderAdapter.ConfigureServer(HostBuilderContext context, IServiceCollection hostServices)
        {
            ConfigureServer(context, hostServices);
        }
        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="context">Host Builder上下文</param>
        /// <param name="hostServices">主机服务</param>
        protected void ConfigureServer(HostBuilderContext context, IServiceCollection hostServices)
        {
            var services = _currentServices;

            CopyGlobalServices(hostServices, services);

            RegisterBasicServices(context, hostServices, services);

            foreach (var configureServicesAction in ConfigureServicesActions)
            {
                configureServicesAction(context, services);
            }

            foreach (var configureServicesAction in ConfigureSupplementServicesActions)
            {
                configureServicesAction(context, services);
            }

            RegisterDefaultServices(context, hostServices, services);

            if (_serviceProviderBuilder == null)
            {
                var serviceFactory = new DefaultServiceProviderFactory();
                var containerBuilder = serviceFactory.CreateBuilder(services);
                ConfigureContainerBuilder(context, containerBuilder);
                _serviceProvider = serviceFactory.CreateServiceProvider(containerBuilder);
            }
            else
            {
                _serviceProvider = _serviceProviderBuilder(context, services);
            }
        }
        /// <summary>
        /// 配置容器构建器
        /// </summary>
        /// <param name="context"></param>
        /// <param name="containerBuilder"></param>
        private void ConfigureContainerBuilder(HostBuilderContext context, object containerBuilder)
        {
            foreach (IConfigureContainerAdapter containerAction in _configureContainerActions)
                containerAction.ConfigureContainer(context, containerBuilder);
        }
        /// <summary>
        /// 复制全局服务
        /// </summary>
        /// <param name="hostServices">主机服务</param>
        /// <param name="services">服务</param>
        private void CopyGlobalServices(IServiceCollection hostServices, IServiceCollection services)
        {
            foreach (var sd in hostServices)
            {
                if (sd.ServiceType == typeof(IHostedService))
                    continue;
                
                CopyGlobalServiceDescriptor(hostServices, services, sd);
            }
        }
        /// <summary>
        /// 赋值全局服务描述
        /// </summary>
        /// <param name="hostServices">主机服务</param>
        /// <param name="services">服务</param>
        /// <param name="sd">服务描述</param>
        private void CopyGlobalServiceDescriptor(IServiceCollection hostServices, IServiceCollection services, ServiceDescriptor sd)
        {
            if (sd.ImplementationInstance != null)
            {
                services.Add(new ServiceDescriptor(sd.ServiceType, sd.ImplementationInstance));
            }
            else if (sd.ImplementationFactory != null)
            {
                services.Add(new ServiceDescriptor(sd.ServiceType, (sp) => GetServiceFromHost(sd.ImplementationFactory), sd.Lifetime));
            }
            else if (sd.ImplementationType != null)
            {
                if (!sd.ServiceType.IsGenericTypeDefinition)
                    services.Add(new ServiceDescriptor(sd.ServiceType, (sp) => _hostServiceProvider.GetService(sd.ServiceType), sd.Lifetime));
                else
                    services.Add(sd);
            }            
        }
        /// <summary>
        /// 从主机获取服务
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        private object GetServiceFromHost(Func<IServiceProvider, object> factory)
        {
            return factory(_hostServiceProvider);
        }
        /// <summary>
        /// 配置服务供应
        /// </summary>
        /// <param name="hostServiceProvider">主机服务供应</param>
        void IServerHostBuilderAdapter.ConfigureServiceProvider(IServiceProvider hostServiceProvider)
        {
            _hostServiceProvider = hostServiceProvider;
        }
        /// <summary>
        /// 注册主机服务
        /// </summary>
        /// <typeparam name="THostedService"></typeparam>
        protected void RegisterHostedService<THostedService>()
            where THostedService : class, IHostedService
        {
            base.HostBuilder.ConfigureServices((context, services) =>
            {
                RegisterHostedService<THostedService>(services);
            });
        }
        /// <summary>
        /// 注册主机服务
        /// </summary>
        /// <typeparam name="THostedService"></typeparam>
        /// <param name="servicesInHost"></param>
        protected override void RegisterHostedService<THostedService>(IServiceCollection servicesInHost)
        {
            _currentServices.AddSingleton<IHostedService, THostedService>();
            _currentServices.AddSingleton<IServerInfo>(s => s.GetService<IHostedService>() as IServerInfo);
            servicesInHost.AddHostedService<THostedService>(s => GetHostedService<THostedService>());
        }
        /// <summary>
        /// 注册默认主机服务
        /// </summary>
        /// <param name="servicesInHost"></param>
        protected override void RegisterDefaultHostedService(IServiceCollection servicesInHost)
        {
            RegisterHostedService<SuperSocketService<TReceivePackage>>(servicesInHost);
        }
        /// <summary>
        /// 获取主机服务
        /// </summary>
        /// <typeparam name="THostedService"></typeparam>
        /// <returns></returns>
        private THostedService GetHostedService<THostedService>()
        {
            return (THostedService)_serviceProvider.GetService<IHostedService>();
        }
        /// <summary>
        /// 使用主机服务
        /// </summary>
        /// <typeparam name="THostedService"></typeparam>
        /// <returns></returns>
        public override ISuperSocketHostBuilder<TReceivePackage> UseHostedService<THostedService>()
        {
            RegisterHostedService<THostedService>();
            return this;
        }
        /// <summary>
        /// 构建
        /// </summary>
        /// <returns></returns>
        public override IHost Build()
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 配置容器
        /// </summary>
        /// <typeparam name="TContainerBuilder">容器构建器的类型</typeparam>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        public override SuperSocketHostBuilder<TReceivePackage> ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            _configureContainerActions.Add(new ConfigureContainerAdapter<TContainerBuilder>(configureDelegate));
            return this;
        }
        /// <summary>
        /// 使用服务供应工厂
        /// </summary>
        /// <typeparam name="TContainerBuilder">容器构建器的类型</typeparam>
        /// <param name="factory">服务供应工厂</param>
        /// <returns></returns>
        public override SuperSocketHostBuilder<TReceivePackage> UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            _serviceProviderBuilder = (context, services) =>
            {
                var containerBuilder = factory.CreateBuilder(services);
                ConfigureContainerBuilder(context, containerBuilder);
                return factory.CreateServiceProvider(containerBuilder);                
            };
            return this;
        }
        /// <summary>
        /// 使用服务供应工厂
        /// </summary>
        /// <typeparam name="TContainerBuilder"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public override SuperSocketHostBuilder<TReceivePackage> UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            _serviceProviderBuilder = (context, services) =>
            {
                var serviceProviderFactory = factory(context);
                var containerBuilder = serviceProviderFactory.CreateBuilder(services);
                ConfigureContainerBuilder(context, containerBuilder);
                return serviceProviderFactory.CreateServiceProvider(containerBuilder);                
            };
            return this;
        }
    }
}