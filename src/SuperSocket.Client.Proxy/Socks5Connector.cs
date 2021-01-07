using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client.Proxy
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc1928
    /// https://en.wikipedia.org/wiki/SOCKS
    /// </summary>
    public class Socks5Connector : ProxyConnectorBase
    {
        private string _username;

        private string _password;

        readonly static byte[] _authenHandshakeRequest = new byte[] { 0x05, 0x02, 0x00, 0x02 };
        /// <summary>
        /// Sock5连接器初始化
        /// </summary>
        /// <param name="proxyEndPoint">代理网络标识</param>
        public Socks5Connector(EndPoint proxyEndPoint)
            : base(proxyEndPoint)
        {

        }
        /// <summary>
        /// Sock5连接器初始化
        /// </summary>
        /// <param name="proxyEndPoint">代理网路标识</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        public Socks5Connector(EndPoint proxyEndPoint, string username, string password)
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
        /// <returns></returns>
        protected override async ValueTask<ConnectState> ConnectProxyAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var channel = state.CreateChannel<Socks5Pack>(new Socks5AuthPipelineFilter(), new ChannelOptions { ReadAsDemand = true });

            channel.Start();

            var packStream = channel.GetPackageStream();

            await channel.SendAsync(_authenHandshakeRequest);

            var response = await packStream.ReceiveAsync();

            if (!HandleResponse(response, Socket5ResponseType.Handshake, out string errorMessage))
            {
                await channel.CloseAsync(CloseReason.ProtocolError);

                return new ConnectState
                {
                    Result = false,
                    Exception = new Exception(errorMessage)
                };
            }

            if (response.Status == 0x02)// need pass auth
            {
                var passAuthenRequest = GetPassAuthenBytes();

                await channel.SendAsync(passAuthenRequest);

                response = await packStream.ReceiveAsync();

                if (!HandleResponse(response, Socket5ResponseType.AuthUserName, out errorMessage))
                {
                    await channel.CloseAsync(CloseReason.ProtocolError);

                    return new ConnectState
                    {
                        Result = false,
                        Exception = new Exception(errorMessage)
                    };
                }
            }

            var endPointRequest = GetEndPointBytes(remoteEndPoint);

            await channel.SendAsync(endPointRequest);

            response = await packStream.ReceiveAsync();

            if (!HandleResponse(response, Socket5ResponseType.AuthEndPoint, out errorMessage))
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
        /// <param name="response">Socket5响应</param>
        /// <param name="responseType">Socket5响应类型</param>
        /// <param name="errorMessage">错误信息</param>
        /// <returns>响应状态；ture:响应成功；false:响应失败；</returns>
        private bool HandleResponse(Socks5Pack response, Socket5ResponseType responseType, out string errorMessage)
        {
            errorMessage = null;

            if (responseType == Socket5ResponseType.Handshake)
            {
                if (response.Status != 0x00 && response.Status != 0x02)
                {
                    errorMessage = $"failed to connect to proxy , protocol violation";
                    return false;
                }
            }
            else if (responseType == Socket5ResponseType.AuthUserName)
            {
                if (response.Status != 0x00)
                {
                    errorMessage = $"failed to connect to proxy ,  username/password combination rejected";
                    return false;
                }
            }
            else
            {
                if (response.Status != 0x00)
                {
                    switch (response.Status)
                    {
                        case (0x02):
                            errorMessage = "connection not allowed by ruleset";
                            break;

                        case (0x03):
                            errorMessage = "network unreachable";
                            break;

                        case (0x04):
                            errorMessage = "host unreachable";
                            break;

                        case (0x05):
                            errorMessage = "connection refused by destination host";
                            break;

                        case (0x06):
                            errorMessage = "TTL expired";
                            break;

                        case (0x07):
                            errorMessage = "command not supported / protocol error";
                            break;

                        case (0x08):
                            errorMessage = "address type not supported";
                            break;

                        default:
                            errorMessage = "general failure";
                            break;
                    }

                    errorMessage = $"failed to connect to proxy ,  { errorMessage }";
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// 获取通过验证的字节码
        /// </summary>
        /// <returns>字节码序列数组</returns>
        private ArraySegment<byte> GetPassAuthenBytes()
        {
            var buffer = new byte[3 + Encoding.ASCII.GetMaxByteCount(_username.Length) + (string.IsNullOrEmpty(_password) ? 0 : Encoding.ASCII.GetMaxByteCount(_password.Length))];
            var actualLength = 0;

            buffer[0] = 0x01;
            var len = Encoding.ASCII.GetBytes(_username, 0, _username.Length, buffer, 2);

            buffer[1] = (byte)len;

            actualLength = len + 2;

            if (!string.IsNullOrEmpty(_password))
            {
                len = Encoding.ASCII.GetBytes(_password, 0, _password.Length, buffer, actualLength + 1);

                buffer[actualLength] = (byte)len;
                actualLength += len + 1;
            }
            else
            {
                buffer[actualLength] = 0x00;
                actualLength++;
            }

            return new ArraySegment<byte>(buffer, 0, actualLength);
        }
        /// <summary>
        /// 获取远程网络标识中的信息
        /// </summary>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <returns>字节码数据</returns>
        private byte[] GetEndPointBytes(EndPoint remoteEndPoint)
        {
            var targetEndPoint = remoteEndPoint;

            byte[] buffer;
            int actualLength;
            int port = 0;

            if (targetEndPoint is IPEndPoint)
            {
                var endPoint = targetEndPoint as IPEndPoint;
                port = endPoint.Port;

                if (endPoint.AddressFamily == AddressFamily.InterNetwork)
                {
                    buffer = new byte[10];
                    buffer[3] = 0x01;
                    Buffer.BlockCopy(endPoint.Address.GetAddressBytes(), 0, buffer, 4, 4);
                }
                else if (endPoint.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    buffer = new byte[22];
                    buffer[3] = 0x04;

                    Buffer.BlockCopy(endPoint.Address.GetAddressBytes(), 0, buffer, 4, 16);
                }
                else
                {
                    throw new Exception("unknown address family");
                }

                actualLength = buffer.Length;
            }
            else
            {
                var endPoint = targetEndPoint as DnsEndPoint;

                port = endPoint.Port;

                var maxLen = 7 + Encoding.ASCII.GetMaxByteCount(endPoint.Host.Length);
                buffer = new byte[maxLen];

                buffer[3] = 0x03;
                buffer[4] = (byte)endPoint.Host.Length;//ԭ��Ϊ0 ����Ϊ�˿ڵĳ���
                actualLength = 5;
                actualLength += Encoding.ASCII.GetBytes(endPoint.Host, 0, endPoint.Host.Length, buffer, actualLength);
                actualLength += 2;
            }

            buffer[0] = 0x05;
            buffer[1] = 0x01;
            buffer[2] = 0x00;

            buffer[actualLength - 2] = (byte)(port / 256);
            buffer[actualLength - 1] = (byte)(port % 256);

            return buffer;
        }

        enum Socket5ResponseType
        {
            Handshake,

            AuthUserName,

            AuthEndPoint,
        }

        public class Socks5Address
        {
            public IPAddress IPAddress { get; set; }

            public string DomainName { get; set; }
        }

        public class Socks5Pack
        {
            public byte Version { get; set; }

            public byte Status { get; set; }

            public byte Reserve { get; set; }

            public Socks5Address DestAddr { get; set; }

            public short DestPort { get; set; }
        }

        public class Socks5AuthPipelineFilter : FixedSizePipelineFilter<Socks5Pack>
        {
            public int AuthStep { get; set; }

            public Socks5AuthPipelineFilter()
                : base(2)
            {

            }
            /// <summary>
            /// 解码Socket5数据包
            /// </summary>
            /// <param name="buffer">数据</param>
            /// <returns>Socket5数据实体</returns>
            protected override Socks5Pack DecodePackage(ref ReadOnlySequence<byte> buffer)
            {
                var reader = new SequenceReader<byte>(buffer);
                reader.TryRead(out byte version);
                reader.TryRead(out byte status);

                if (AuthStep == 0)
                    NextFilter = new Socks5AuthPipelineFilter { AuthStep = 1 };
                else
                    NextFilter = new Socks5AddressPipelineFilter();

                return new Socks5Pack
                {
                    Version = version,
                    Status = status
                };
            }
        }

        public class Socks5AddressPipelineFilter : FixedHeaderPipelineFilter<Socks5Pack>
        {
            public Socks5AddressPipelineFilter()
                : base(5)
            {

            }
            /// <summary>
            /// 从Socket5协议头中获取数据体的长度
            /// </summary>
            /// <param name="buffer">数据</param>
            /// <returns>协议头中数据的长度</returns>
            protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
            {
                var reader = new SequenceReader<byte>(buffer);
                reader.Advance(3);
                reader.TryRead(out byte addressType);

                if (addressType == 0x01)
                    return 6 - 1;

                if (addressType == 0x04)
                    return 18 - 1;

                if (addressType == 0x03)
                {
                    reader.TryRead(out byte domainLen);
                    return domainLen + 2;
                }

                throw new Exception($"Unsupported addressType: {addressType}");
            }
            /// <summary>
            /// 解码Socket5数据包
            /// </summary>
            /// <param name="buffer">数据</param>
            /// <returns>Socket5数据实体</returns>
            protected override Socks5Pack DecodePackage(ref ReadOnlySequence<byte> buffer)
            {
                var reader = new SequenceReader<byte>(buffer);
                reader.TryRead(out byte version);
                reader.TryRead(out byte status);
                reader.TryRead(out byte reserve);

                reader.TryRead(out byte addressType);

                var address = new Socks5Address();

                if (addressType == 0x01)
                {
                    var addrLen = 4;
                    address.IPAddress = new IPAddress(reader.Sequence.Slice(reader.Consumed, addrLen).ToArray());
                    reader.Advance(addrLen);
                }
                else if (addressType == 0x04)
                {
                    var addrLen = 16;
                    address.IPAddress = new IPAddress(reader.Sequence.Slice(reader.Consumed, addrLen).ToArray());
                    reader.Advance(addrLen);
                }
                else if (addressType == 0x03)
                {
                    reader.TryRead(out byte addrLen);
                    var seq = reader.Sequence.Slice(reader.Consumed, addrLen);
                    address.DomainName = seq.GetString(Encoding.ASCII);
                    reader.Advance(addrLen);
                }
                else
                {
                    throw new Exception($"Unsupported addressType: {addressType}");
                }

                reader.TryReadBigEndian(out short port);

                return new Socks5Pack
                {
                    Version = version,
                    Status = status,
                    Reserve = reserve,
                    DestAddr = address,
                    DestPort = port
                };
            }
        }
    }
}