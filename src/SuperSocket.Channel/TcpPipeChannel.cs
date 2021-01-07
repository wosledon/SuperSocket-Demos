using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Buffers;
using System.Collections.Generic;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public class TcpPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
    {

        private Socket _socket;

        private List<ArraySegment<byte>> _segmentsForSend;
        /// <summary>
        /// TCP管道初始化
        /// </summary>
        /// <param name="socket">Socket对象</param>
        /// <param name="pipelineFilter">命令行</param>
        /// <param name="options">选项设置</param>
        public TcpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
            : base(pipelineFilter, options)
        {
            _socket = socket;
            RemoteEndPoint = socket.RemoteEndPoint;
            LocalEndPoint = socket.LocalEndPoint;
        }
        /// <summary>
        /// 触发关闭事件
        /// </summary>
        protected override void OnClosed()
        {
            _socket = null;
            base.OnClosed();
        }
        /// <summary>
        /// 数据充满管道
        /// </summary>
        /// <param name="memory">内存</param>
        /// <param name="cancellationToken">异步操作标识</param>
        /// <returns></returns>
        protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            return await ReceiveAsync(_socket, memory, SocketFlags.None, cancellationToken);
        }
        /// <summary>
        /// 接收数据（异步）
        /// </summary>
        /// <param name="socket">Socket对象</param>
        /// <param name="memory">内存</param>
        /// <param name="socketFlags">Socket标识</param>
        /// <param name="cancellationToken">异步操作Token</param>
        /// <returns></returns>
        private async ValueTask<int> ReceiveAsync(Socket socket, Memory<byte> memory, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            return await socket
                .ReceiveAsync(GetArrayByMemory((ReadOnlyMemory<byte>)memory), socketFlags, cancellationToken)
                .ConfigureAwait(false);
        }
        /// <summary>
        /// 发送所有数据
        /// </summary>
        /// <param name="buffer">只读字节码序列</param>
        /// <param name="cancellationToken">异步操作Token</param>
        /// <returns></returns>
        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            if (buffer.IsSingleSegment)
            {
                return await _socket
                    .SendAsync(GetArrayByMemory(buffer.First), SocketFlags.None, cancellationToken)
                    .ConfigureAwait(false);
            }
            
            if (_segmentsForSend == null)
            {
                _segmentsForSend = new List<ArraySegment<byte>>();
            }
            else
            {
                _segmentsForSend.Clear();
            }

            var segments = _segmentsForSend;

            foreach (var piece in buffer)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _segmentsForSend.Add(GetArrayByMemory(piece));
            }

            cancellationToken.ThrowIfCancellationRequested();
            
            return await _socket
                .SendAsync(_segmentsForSend, SocketFlags.None)
                .ConfigureAwait(false);
        }
        /// <summary>
        /// 关闭管道
        /// </summary>
        protected override void Close()
        {
            var socket = _socket;

            if (socket == null)
                return;

            if (Interlocked.CompareExchange(ref _socket, null, socket) == socket)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    socket.Close();
                }
            }
        }
        /// <summary>
        /// 是否忽略异常
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected override bool IsIgnorableException(Exception e)
        {
            if (base.IsIgnorableException(e))
                return true;

            if (e is SocketException se)
            {
                if (se.IsIgnorableSocketException())
                    return true;
            }

            return false;
        }
    }
}
