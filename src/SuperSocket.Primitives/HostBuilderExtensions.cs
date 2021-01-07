using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;


namespace SuperSocket
{
public static class HostBuilderExtensions
{
/// <summary>
/// 
/// </summary>
/// <param name="hostBuilder"></param>
/// <returns></returns>
public static ISuperSocketHostBuilder AsSuperSocketBuilder(this IHostBuilder hostBuilder)
{
    return hostBuilder as ISuperSocketHostBuilder;
}
/// <summary>
/// 使用中间件
/// </summary>
/// <typeparam name="TMiddleware">中间件类型</typeparam>
/// <param name="builder">Host Builder</param>
/// <returns></returns>
public static ISuperSocketHostBuilder UseMiddleware<TMiddleware>(this ISuperSocketHostBuilder builder)
    where TMiddleware : class, IMiddleware
{
    return builder.ConfigureServices((ctx, services) => 
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, TMiddleware>());
    }).AsSuperSocketBuilder();
}
/// <summary>
/// 使用中间件
/// </summary>
/// <typeparam name="TMiddleware">中间件类型</typeparam>
/// <param name="builder">Host Builder</param>
/// <param name="implementationFactory">实施工厂</param>
/// <returns></returns>
public static ISuperSocketHostBuilder UseMiddleware<TMiddleware>(this ISuperSocketHostBuilder builder, Func<IServiceProvider, TMiddleware> implementationFactory)
    where TMiddleware : class, IMiddleware
{
    return builder.ConfigureServices((ctx, services) => 
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, TMiddleware>(implementationFactory));
    }).AsSuperSocketBuilder();
}
}
}
