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
    public class MultipleServerHostBuilder : HostBuilderAdapter<MultipleServerHostBuilder>
    {
        private List<IServerHostBuilderAdapter> _hostBuilderAdapters = new List<IServerHostBuilderAdapter>();
        /// <summary>
        /// 初始化多服务器主机构建器
        /// </summary>
        private MultipleServerHostBuilder()
            : this(args: null)
        {

        }
        /// <summary>
        /// 初始化多服务器主机构建器
        /// </summary>
        /// <param name="args">参数</param>
        private MultipleServerHostBuilder(string[] args)
            : base(args)
        {

        }
        /// <summary>
        /// 初始化多服务器主机构建器
        /// </summary>
        /// <param name="hostBuilder">Host Builder</param>
        internal MultipleServerHostBuilder(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {

        }
        /// <summary>
        /// 配置主机
        /// </summary>
        /// <param name="context">主机构建上下文</param>
        /// <param name="hostServices">主机服务</param>
        protected virtual void ConfigureServers(HostBuilderContext context, IServiceCollection hostServices)
        {
            foreach (var adapter in _hostBuilderAdapters)
            {
                adapter.ConfigureServer(context, hostServices);
            }
        }
        /// <summary>
        /// 构建
        /// </summary>
        /// <returns></returns>
        public override IHost Build()
        {
            this.ConfigureServices(ConfigureServers);

            var host = base.Build();
            var services = host.Services;

            foreach (var adapter in _hostBuilderAdapters)
            {
                adapter.ConfigureServiceProvider(services);
            }
            
            return host;
        }
        /// <summary>
        /// 创建
        /// </summary>
        /// <returns></returns>
        public static MultipleServerHostBuilder Create()
        {
            return Create(args: null);
        }
        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static MultipleServerHostBuilder Create(string[] args)
        {
            return new MultipleServerHostBuilder(args);
        }
        /// <summary>
        /// 创建服务器主机构建器
        /// </summary>
        /// <typeparam name="TReceivePackage"></typeparam>
        /// <param name="hostBuilderDelegate"></param>
        /// <returns></returns>
        private ServerHostBuilderAdapter<TReceivePackage> CreateServerHostBuilder<TReceivePackage>(Action<SuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate)
            where TReceivePackage : class
        {
            var hostBuilder = new ServerHostBuilderAdapter<TReceivePackage>(this);            
            hostBuilderDelegate(hostBuilder);
            return hostBuilder;
        }
        /// <summary>
        /// 添加服务器
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <param name="hostBuilderDelegate">主机构建委托</param>
        /// <returns></returns>
        public MultipleServerHostBuilder AddServer<TReceivePackage>(Action<SuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate)
            where TReceivePackage : class
        {
            var hostBuilder = CreateServerHostBuilder<TReceivePackage>(hostBuilderDelegate);
            _hostBuilderAdapters.Add(hostBuilder);
            return this;
        }
        /// <summary>
        /// 添加服务器
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <typeparam name="TPipelineFilter">管道筛选器</typeparam>
        /// <param name="hostBuilderDelegate">主机构建委托</param>
        /// <returns></returns>
        public MultipleServerHostBuilder AddServer<TReceivePackage, TPipelineFilter>(Action<SuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate)
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {            
            var hostBuilder = CreateServerHostBuilder<TReceivePackage>(hostBuilderDelegate);
            _hostBuilderAdapters.Add(hostBuilder);
            hostBuilder.UsePipelineFilter<TPipelineFilter>();
            return this;
        }
        /// <summary>
        /// 添加服务器
        /// </summary>
        /// <param name="hostBuilderAdapter">主机构建适配器</param>
        /// <returns></returns>
        public MultipleServerHostBuilder AddServer(IServerHostBuilderAdapter hostBuilderAdapter)
        {            
            _hostBuilderAdapters.Add(hostBuilderAdapter);
            return this;
        }
        /// <summary>
        /// 添加服务器
        /// </summary>
        /// <typeparam name="TSuperSocketService">SuperSocket服务</typeparam>
        /// <typeparam name="TReceivePackage">接收包的类型</typeparam>
        /// <typeparam name="TPipelineFilter">管道筛选器</typeparam>
        /// <param name="hostBuilderDelegate">主机构建委托</param>
        /// <returns></returns>
        public MultipleServerHostBuilder AddServer<TSuperSocketService, TReceivePackage, TPipelineFilter>(Action<SuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate)
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
            where TSuperSocketService : SuperSocketService<TReceivePackage>
        {
            var hostBuilder = CreateServerHostBuilder<TReceivePackage>(hostBuilderDelegate);

            _hostBuilderAdapters.Add(hostBuilder);

            hostBuilder
                .UsePipelineFilter<TPipelineFilter>()
                .UseHostedService<TSuperSocketService>();
            return this;
        }
    }
}