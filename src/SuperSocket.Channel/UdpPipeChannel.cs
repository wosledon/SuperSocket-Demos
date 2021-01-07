using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public class UdpPipeChannel<TPackageInfo> : VirtualChannel<TPackageInfo>, IChannelWithSessionIdentifier
    {
        private Socket _socket;

        private IPEndPoint _remoteEndPoint;
        /// <summary>
        /// 初始化Udp管道
        /// </summary>
        /// <param name="socket">Socket对象</param>
        /// <param name="pipelineFilter">管道筛选器</param>
        /// <param name="options">选项</param>
        /// <param name="remoteEndPoint">远程网络标识</param>
        public UdpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, IPEndPoint remoteEndPoint)
            : this(socket, pipelineFilter, options, remoteEndPoint, $"{remoteEndPoint.Address}:{remoteEndPoint.Port}")
        {

        }
        /// <summary>
        /// 初始化Udp管道
        /// </summary>
        /// <param name="socket">Socket通道</param>
        /// <param name="pipelineFilter">管道筛选器</param>
        /// <param name="options">选项</param>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="sessionIdentifier">Session标识符</param>
        public UdpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, IPEndPoint remoteEndPoint, string sessionIdentifier)
            : base(pipelineFilter, options)
        {
            _socket = socket;
            _remoteEndPoint = remoteEndPoint;
            SessionIdentifier = sessionIdentifier;
        }

        public string SessionIdentifier { get; }
        /// <summary>
        /// 关闭
        /// </summary>
        protected override void Close()
        {
            WriteEOFPackage();
        }
        /// <summary>
        /// 数据充满管道
        /// </summary>
        /// <param name="memory">内存</param>
        /// <param name="cancellationToken">异步操作标识Token</param>
        /// <returns></returns>
        protected override ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buffer">只读字节码序列</param>
        /// <param name="cancellationToken">异步操作标识Token</param>
        /// <returns>发送数据的长度</returns>
        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            var total = 0;

            foreach (var piece in buffer)
            {
                total += await _socket.SendToAsync(GetArrayByMemory<byte>(piece), SocketFlags.None, _remoteEndPoint);
            }

            return total;
        }
    }
}