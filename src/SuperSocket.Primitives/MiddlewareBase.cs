
using System.Threading.Tasks;

namespace SuperSocket
{
    public abstract class MiddlewareBase : IMiddleware
    {
        /// <summary>
        /// 中间件列表
        /// </summary>
        public int Order { get; protected set; } = 0;
        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="server">服务器</param>
        public virtual void Start(IServer server)
        {

        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="server">服务器</param>
        public virtual void Shutdown(IServer server)
        {
            
        }
        /// <summary>
        /// 注册Session
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public virtual ValueTask<bool> RegisterSession(IAppSession session)
        {
            return new ValueTask<bool>(true);
        }
    }
}