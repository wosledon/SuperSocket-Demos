using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public class SslStreamConnector : ConnectorBase
    {
        /// <summary>
        /// SSL认证选项
        /// </summary>
        public SslClientAuthenticationOptions Options { get; private set; }
        /// <summary>
        /// 初始化SSL连接器
        /// </summary>
        /// <param name="options">SSL认证选项</param>
        public SslStreamConnector(SslClientAuthenticationOptions options)
            : base()
        {
            Options = options;
        }
        /// <summary>
        /// 初始化SSL连接器
        /// </summary>
        /// <param name="options">SSL认证选项</param>
        /// <param name="nextConnector">连接器</param>
        public SslStreamConnector(SslClientAuthenticationOptions options, IConnector nextConnector)
            : base(nextConnector)
        {
            Options = options;
        }
        /// <summary>
        /// 建立SSL连接（异步）
        /// </summary>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="state">连接状态</param>
        /// <param name="cancellationToken">异步操作Token</param>
        /// <returns></returns>
        protected override async ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var targetHost = Options.TargetHost;

            if (string.IsNullOrEmpty(targetHost))
            {
                if (remoteEndPoint is DnsEndPoint remoteDnsEndPoint)
                    targetHost = remoteDnsEndPoint.Host;
                else if (remoteEndPoint is IPEndPoint remoteIPEndPoint)
                    targetHost = remoteIPEndPoint.Address.ToString();

                Options.TargetHost = targetHost;
            }

            var socket = state.Socket;

            if (socket == null)
                throw new Exception("Socket from previous connector is null.");
            
            try
            {
                var stream = new SslStream(new NetworkStream(socket, true), false);
                await stream.AuthenticateAsClientAsync(Options, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return ConnectState.CancelledState;

                state.Stream = stream;
                return state;
            }
            catch (Exception e)
            {
                return new ConnectState
                {
                    Result = false,
                    Exception = e
                };
            }
        }
    }
}