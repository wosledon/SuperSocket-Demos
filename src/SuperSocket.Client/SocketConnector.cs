using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public class SocketConnector : ConnectorBase
    {
        /// <summary>
        /// 标识网络地址
        /// </summary>
        public IPEndPoint LocalEndPoint { get; private set; }

        public SocketConnector()
            : base()
        {

        }

        public SocketConnector(IConnector nextConnector)
            : base(nextConnector)
        {

        }

        public SocketConnector(IPEndPoint localEndPoint)
            : base()
        {
            LocalEndPoint = localEndPoint;
        }

        public SocketConnector(IPEndPoint localEndPoint, IConnector nextConnector)
            : base(nextConnector)
        {
            LocalEndPoint = localEndPoint;
        }
        /// <summary>
        /// 建立socket连接（异步）
        /// </summary>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="state">连接状态</param>
        /// <param name="cancellationToken">异步操作的Token，用于取消异步操作</param>
        /// <returns>连接状态</returns>
        protected override async ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                var localEndPoint = LocalEndPoint;

                if (localEndPoint != null)
                {
                    socket.ExclusiveAddressUse = false;
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);             
                    socket.Bind(localEndPoint);
                }

                await socket.ConnectAsync(remoteEndPoint);
            }
            catch (Exception e)
            {
                return new ConnectState
                {
                    Result = false,
                    Exception = e
                };
            }

            return new ConnectState
            {
                Result = true,
                Socket = socket
            };            
        }
    }
}