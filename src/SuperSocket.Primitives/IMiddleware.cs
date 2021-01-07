
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IMiddleware
    {
        /// <summary>
        /// 中间件清单
        /// </summary>
        int Order { get; }
        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="server"></param>
        void Start(IServer server);
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="server"></param>
        void Shutdown(IServer server);
        /// <summary>
        /// 注册Session
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        ValueTask<bool> RegisterSession(IAppSession session);
    }
}