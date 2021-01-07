using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    public abstract class HostBuilderAdapter<THostBuilder> : IHostBuilder
        where THostBuilder : HostBuilderAdapter<THostBuilder>
    {
        protected IHostBuilder HostBuilder { get; private set; }
        /// <summary>
        /// 初始化Host Builder适配器
        /// </summary>
        public HostBuilderAdapter()
            : this(args: null)
        {
            
        }
        /// <summary>
        /// 初始化Host Builder适配器
        /// </summary>
        /// <param name="args">参数</param>
        public HostBuilderAdapter(string[] args)
            : this(Host.CreateDefaultBuilder(args))
        {
            
        }
        /// <summary>
        /// 初始化Host Builder适配器
        /// </summary>
        /// <param name="hostBuilder">Host Builder</param>
        public HostBuilderAdapter(IHostBuilder hostBuilder)
        {
            HostBuilder = hostBuilder;
        }

        public IDictionary<object, object> Properties => HostBuilder.Properties;
        /// <summary>
        /// 建立
        /// </summary>
        /// <returns></returns>
        public virtual IHost Build()
        {
            return HostBuilder.Build();
        }
        /// <summary>
        /// 配置应用程序配置
        /// </summary>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        IHostBuilder IHostBuilder.ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            return ConfigureAppConfiguration(configureDelegate);
        }
        /// <summary>
        /// 配置应用程序配置
        /// </summary>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        public virtual THostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            HostBuilder.ConfigureAppConfiguration(configureDelegate);
            return this as THostBuilder;
        }
        /// <summary>
        /// 配置容器
        /// </summary>
        /// <typeparam name="TContainerBuilder">容器建造器的类型</typeparam>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        IHostBuilder IHostBuilder.ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            return ConfigureContainer<TContainerBuilder>(configureDelegate);
        }
        /// <summary>
        /// 配置容器
        /// </summary>
        /// <typeparam name="TContainerBuilder">容器建造器的类型</typeparam>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        public virtual THostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            HostBuilder.ConfigureContainer<TContainerBuilder>(configureDelegate);
            return this as THostBuilder;
        }
        /// <summary>
        /// 配置Host的配置
        /// </summary>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        IHostBuilder IHostBuilder.ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            return ConfigureHostConfiguration(configureDelegate);
        }
        /// <summary>
        /// 配置Host的配置
        /// </summary>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        public THostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            HostBuilder.ConfigureHostConfiguration(configureDelegate);
            return this as THostBuilder;
        }
        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        IHostBuilder IHostBuilder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            return ConfigureServices(configureDelegate);
        }
        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        public virtual THostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            HostBuilder.ConfigureServices(configureDelegate);
            return this as THostBuilder;
        }
        /// <summary>
        /// 使用服务供应工厂
        /// </summary>
        /// <typeparam name="TContainerBuilder">容器建造器</typeparam>
        /// <param name="factory">服务提供工厂</param>
        /// <returns></returns>
        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            return UseServiceProviderFactory<TContainerBuilder>(factory);
        }
        /// <summary>
        /// 使用服务提供工厂
        /// </summary>
        /// <typeparam name="TContainerBuilder">容器建造器</typeparam>
        /// <param name="factory">容器建造器</param>
        /// <returns></returns>
        public virtual THostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            HostBuilder.UseServiceProviderFactory<TContainerBuilder>(factory);
            return this as THostBuilder;
        }
        /// <summary>
        /// 使用服务提供工厂
        /// </summary>
        /// <typeparam name="TContainerBuilder">容器建造器</typeparam>
        /// <param name="factory">容器建造器</param>
        /// <returns></returns>
        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            return UseServiceProviderFactory<TContainerBuilder>(factory);
        }
        /// <summary>
        /// 使用服务提供工厂
        /// </summary>
        /// <typeparam name="TContainerBuilder">容器建造器</typeparam>
        /// <param name="factory">容器建造器</param>
        /// <returns></returns>
        public virtual THostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            HostBuilder.UseServiceProviderFactory<TContainerBuilder>(factory);
            return this as THostBuilder;
        }
    }
}