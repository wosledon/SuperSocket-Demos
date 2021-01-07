using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Client;

namespace SuperSocket.Client.Proxy
{
    public class Socks4Connector : ConnectorBase
    {
        /// <summary>
        /// 异步连接
        /// </summary>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="state">连接状态</param>
        /// <param name="cancellationToken">异步操作标识Token</param>
        /// <returns></returns>
        protected override ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}