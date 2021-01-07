using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public abstract class ConnectorBase : IConnector
    {
        public IConnector NextConnector { get; private set; }

        public ConnectorBase()
        {

        }
        /// <summary>
        /// 初始化连接器
        /// </summary>
        /// <param name="nextConnector">下一个连接器</param>
        public ConnectorBase(IConnector nextConnector)
            : this()
        {
            NextConnector = nextConnector;
        }
        /// <summary>
        /// 抽象方法：发起连接（异步）
        /// </summary>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="state">连接状态</param>
        /// <param name="cancellationToken">用于取消异步操作</param>
        /// <returns></returns>
        protected abstract ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken);
        /// <summary>
        /// 发起连接（异步）
        /// </summary>
        /// <param name="remoteEndPoint">远程标识网络地址</param>
        /// <param name="state">连接状态</param>
        /// <param name="cancellationToken">异步操作的Token，用于取消异步操作</param>
        /// <returns>连接状态</returns>
        async ValueTask<ConnectState> IConnector.ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var result = await ConnectAsync(remoteEndPoint, state, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return ConnectState.CancelledState;

            var nextConnector = NextConnector;

            if (!result.Result || nextConnector == null)
                return result;            

            return await nextConnector.ConnectAsync(remoteEndPoint, result, cancellationToken);
        }
    }
}