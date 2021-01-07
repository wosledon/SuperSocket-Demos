using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client.Proxy
{

    public abstract class ProxyConnectorBase : ConnectorBase
    {
        private EndPoint _proxyEndPoint;
        /// <summary>
        /// 初始化代理连接器
        /// </summary>
        /// <param name="proxyEndPoint">代理网络标识</param>
        public ProxyConnectorBase(EndPoint proxyEndPoint)
        {
            _proxyEndPoint = proxyEndPoint;
        }
        /// <summary>
        /// 异步连接代理
        /// </summary>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="state">连接状态</param>
        /// <param name="cancellationToken">异步操作标识Token</param>
        /// <returns></returns>
        protected abstract ValueTask<ConnectState> ConnectProxyAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken);
        /// <summary>
        /// 异步连接
        /// </summary>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="state">连接状态</param>
        /// <param name="cancellationToken">异步操作表示</param>
        /// <returns></returns>
        protected override async ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var socketConnector = new SocketConnector() as IConnector;
            var proxyEndPoint = _proxyEndPoint;

            ConnectState result;
            
            try
            {
                result = await socketConnector.ConnectAsync(proxyEndPoint, null, cancellationToken);
                
                if (!result.Result)
                    return result;
            }
            catch (Exception e)
            {
                return new ConnectState
                {
                    Result = false,
                    Exception = e
                };
            }

            return await ConnectProxyAsync(remoteEndPoint, state, cancellationToken);
        }
    }
}