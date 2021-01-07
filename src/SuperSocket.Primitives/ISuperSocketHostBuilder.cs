using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket.ProtoBase;

namespace SuperSocket
{
    public interface ISuperSocketHostBuilder : IHostBuilder
    {
        /// <summary>
        /// 配置补充服务
        /// </summary>
        /// <param name="configureDelegate">配置委托</param>
        /// <returns></returns>
        ISuperSocketHostBuilder ConfigureSupplementServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);
    }

    public interface ISuperSocketHostBuilder<TReceivePackage> : ISuperSocketHostBuilder
    {
        /// <summary>
        /// 配置服务器设置
        /// </summary>
        /// <param name="serverOptionsReader"></param>
        /// <returns></returns>
        ISuperSocketHostBuilder<TReceivePackage> ConfigureServerOptions(Func<HostBuilderContext, IConfiguration, IConfiguration> serverOptionsReader);
        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="configureDelegate"></param>
        /// <returns></returns>
        new ISuperSocketHostBuilder<TReceivePackage> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);
        /// <summary>
        /// 配置补充服务
        /// </summary>
        /// <param name="configureDelegate"></param>
        /// <returns></returns>
        new ISuperSocketHostBuilder<TReceivePackage> ConfigureSupplementServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);
        /// <summary>
        /// 使用中间件
        /// </summary>
        /// <typeparam name="TMiddleware">中间件的类型</typeparam>
        /// <returns></returns>
        ISuperSocketHostBuilder<TReceivePackage> UseMiddleware<TMiddleware>()
            where TMiddleware : class, IMiddleware;
        /// <summary>
        /// 使用管道筛选器
        /// </summary>
        /// <typeparam name="TPipelineFilter">管道筛选器</typeparam>
        /// <returns></returns>
        ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilter<TPipelineFilter>()
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new();
        /// <summary>
        /// 使用管道筛选器工厂
        /// </summary>
        /// <typeparam name="TPipelineFilterFactory">管道筛选器工厂</typeparam>
        /// <returns></returns>
        ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TPipelineFilterFactory>()
            where TPipelineFilterFactory : class, IPipelineFilterFactory<TReceivePackage>;
        /// <summary>
        /// 使用Host服务
        /// </summary>
        /// <typeparam name="THostedService"></typeparam>
        /// <returns></returns>
        ISuperSocketHostBuilder<TReceivePackage> UseHostedService<THostedService>()
            where THostedService : class, IHostedService;
        /// <summary>
        /// 使用包解码
        /// </summary>
        /// <typeparam name="TPackageDecoder">包解码类型</typeparam>
        /// <returns></returns>
        ISuperSocketHostBuilder<TReceivePackage> UsePackageDecoder<TPackageDecoder>()
            where TPackageDecoder : class, IPackageDecoder<TReceivePackage>;
        /// <summary>
        /// 使用包处理调度
        /// </summary>
        /// <typeparam name="TPackageHandlingScheduler"></typeparam>
        /// <returns></returns>
        ISuperSocketHostBuilder<TReceivePackage> UsePackageHandlingScheduler<TPackageHandlingScheduler>()
            where TPackageHandlingScheduler : class, IPackageHandlingScheduler<TReceivePackage>;
        /// <summary>
        /// 使用Session工厂
        /// </summary>
        /// <typeparam name="TSessionFactory"></typeparam>
        /// <returns></returns>
        ISuperSocketHostBuilder<TReceivePackage> UseSessionFactory<TSessionFactory>()
            where TSessionFactory : class, ISessionFactory;
        /// <summary>
        /// 使用Session
        /// </summary>
        /// <typeparam name="TSession"></typeparam>
        /// <returns></returns>
        ISuperSocketHostBuilder<TReceivePackage> UseSession<TSession>()
            where TSession : IAppSession;
        /// <summary>
        /// 使用包处理上下文访问器
        /// </summary>
        /// <returns></returns>
        ISuperSocketHostBuilder<TReceivePackage> UsePackageHandlingContextAccessor();
    }
}