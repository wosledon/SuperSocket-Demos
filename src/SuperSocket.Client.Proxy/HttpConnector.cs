using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client.Proxy
{
    public class HttpConnector : ProxyConnectorBase
    {
        private const string _requestTemplate = "CONNECT {0}:{1} HTTP/1.1\r\nHost: {0}:{1}\r\nProxy-Connection: Keep-Alive\r\n";
        private const string _responsePrefix = "HTTP/1.";
        private const char _space = ' ';
        private string _username;
        private string _password;
        /// <summary>
        /// 初始化http连接器
        /// </summary>
        /// <param name="proxyEndPoint">代理网络标识</param>
        public HttpConnector(EndPoint proxyEndPoint)
            : base(proxyEndPoint)
        {

        }
        /// <summary>
        /// 初始化http连接器
        /// </summary>
        /// <param name="proxyEndPoint">代理网络标识</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        public HttpConnector(EndPoint proxyEndPoint, string username, string password)
            : this(proxyEndPoint)
        {
            _username = username;
            _password = password;
        }
        /// <summary>
        /// 连接代理
        /// </summary>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="state">连接状态</param>
        /// <param name="cancellationToken">异步操作标识Token</param>
        /// <returns>连接状态</returns>
        protected override async ValueTask<ConnectState> ConnectProxyAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var encoding = Encoding.ASCII;
            var request = string.Empty;
            var channel = state.CreateChannel<TextPackageInfo>(new LinePipelineFilter(encoding), new ChannelOptions { ReadAsDemand = true });

            channel.Start();

            if (remoteEndPoint is DnsEndPoint dnsEndPoint)
            {
                request = string.Format(_requestTemplate, dnsEndPoint.Host, dnsEndPoint.Port);
            }
            else if (remoteEndPoint is IPEndPoint ipEndPoint)
            {
                request = string.Format(_requestTemplate, ipEndPoint.Address, ipEndPoint.Port);
            }
            else
            {
                return new ConnectState
                {
                    Result = false,
                    Exception = new Exception($"The endpint type {remoteEndPoint.GetType().ToString()} is not supported.")
                };
            }

            // send request
            await channel.SendAsync((writer) =>
            {
                writer.Write(request, encoding);

                if (!string.IsNullOrEmpty(_username) || !string.IsNullOrEmpty(_password))
                {
                    writer.Write("Proxy-Authorization: Basic ", encoding);
                    writer.Write(Convert.ToBase64String(encoding.GetBytes($"{_username}:{_password}")), encoding);
                    writer.Write("\r\n\r\n", encoding);
                }
                else
                {
                    writer.Write("\r\n", encoding);
                }
            });

            var packStream = channel.GetPackageStream();
            var p = await packStream.ReceiveAsync();

            if (!HandleResponse(p, out string errorMessage))
            {
                await channel.CloseAsync(CloseReason.ProtocolError);

                return new ConnectState
                {
                    Result = false,
                    Exception = new Exception(errorMessage)
                };
            }

            await channel.DetachAsync();
            return state;
        }
        /// <summary>
        /// 处理响应
        /// </summary>
        /// <param name="p">文本包信息</param>
        /// <param name="message">消息</param>
        /// <returns>响应状态；true:成功；false:失败；</returns>
        private bool HandleResponse(TextPackageInfo p, out string message)
        {
            message = string.Empty;

            if (p == null)
                return false;

            var pos = p.Text.IndexOf(_space);

            // validating response
            if (!p.Text.StartsWith(_responsePrefix, StringComparison.OrdinalIgnoreCase) || pos <= 0)
            {
                message = "Invalid response";
                return false;
            }

            if (!int.TryParse(p.Text.AsSpan().Slice(pos + 1, 3), out var statusCode))
            {
                message = "Invalid response";
                return false;
            }

            if (statusCode < 200 || statusCode > 299)
            {
                message = $"Invalid status code {statusCode}";
                return false;
            }

            return true;
        }
    }
}