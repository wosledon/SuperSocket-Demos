using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SuperSocket
{
public static class SessionContainerExtensions
{
/// <summary>
/// 转同步Session容器
/// </summary>
/// <param name="asyncSessionContainer">异步Session容器</param>
/// <returns></returns>
public static ISessionContainer ToSyncSessionContainer(this IAsyncSessionContainer asyncSessionContainer)
{
    return new AsyncToSyncSessionContainerWrapper(asyncSessionContainer);
}
/// <summary>
/// 转异步Session容器
/// </summary>
/// <param name="syncSessionContainer">同步Session容器</param>
/// <returns></returns>
public static IAsyncSessionContainer ToAsyncSessionContainer(this ISessionContainer syncSessionContainer)
{
    return new SyncToAsyncSessionContainerWrapper(syncSessionContainer);
}
/// <summary>
/// 获取Session容器
/// </summary>
/// <param name="serviceProvider">ServiceProvider</param>
/// <returns>Session容器</returns>
[Obsolete("Please use the method server.GetSessionContainer() instead.")]
public static ISessionContainer GetSessionContainer(this IServiceProvider serviceProvider)
{
    var sessionContainer = serviceProvider.GetServices<IMiddleware>()
        .OfType<ISessionContainer>()
        .FirstOrDefault();

    if (sessionContainer != null)
        return sessionContainer;

    var asyncSessionContainer = serviceProvider.GetServices<IMiddleware>()
        .OfType<IAsyncSessionContainer>()
        .FirstOrDefault();

    return asyncSessionContainer?.ToSyncSessionContainer();
}
/// <summary>
/// 获取异步Session容器
/// </summary>
/// <param name="serviceProvider">ServiceProvider</param>
/// <returns>异步Session容器</returns>
[Obsolete("Please use the method server.GetSessionContainer() instead.")]
public static IAsyncSessionContainer GetAsyncSessionContainer(this IServiceProvider serviceProvider)
{
    var asyncSessionContainer = serviceProvider.GetServices<IMiddleware>()
        .OfType<IAsyncSessionContainer>()
        .FirstOrDefault();

    if (asyncSessionContainer != null)
        return asyncSessionContainer;

    var sessionContainer = serviceProvider.GetServices<IMiddleware>()
        .OfType<ISessionContainer>()
        .FirstOrDefault();

    return sessionContainer?.ToAsyncSessionContainer(); 
}
/// <summary>
/// 获取Session容器
/// </summary>
/// <param name="server">服务器</param>
/// <returns></returns>
public static ISessionContainer GetSessionContainer(this IServerInfo server)
{
    #pragma warning disable CS0618
    return server.ServiceProvider.GetSessionContainer();
    #pragma warning restore CS0618
}
/// <summary>
/// 获取异步Session容器
/// </summary>
/// <param name="server">服务器</param>
/// <returns></returns>
public static IAsyncSessionContainer GetAsyncSessionContainer(this IServerInfo server)
{
    #pragma warning disable CS0618
    return server.ServiceProvider.GetAsyncSessionContainer();
    #pragma warning restore CS0618
}
}
}