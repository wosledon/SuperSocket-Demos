using System;
using System.Buffers;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public class StreamPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
    {
        private Stream _stream;
        /// <summary>
        /// 初始化字节码管道
        /// </summary>
        /// <param name="stream">字节码流</param>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="pipelineFilter">命令行</param>
        /// <param name="options">选项</param>
        public StreamPipeChannel(Stream stream, EndPoint remoteEndPoint, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
            : this(stream, remoteEndPoint, null, pipelineFilter, options)
        {
            
        }
        /// <summary>
        /// 初始化字节码管道
        /// </summary>
        /// <param name="stream">字节码流</param>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="localEndPoint">本地网络标识</param>
        /// <param name="pipelineFilter">命令行</param>
        /// <param name="options">选项</param>
        public StreamPipeChannel(Stream stream, EndPoint remoteEndPoint, EndPoint localEndPoint, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
            : base(pipelineFilter, options)
        {
            _stream = stream;
            RemoteEndPoint = remoteEndPoint;
            LocalEndPoint = localEndPoint;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        protected override void Close()
        {
            _stream.Close();
        }

        /// <summary>
        /// 触发关闭事件
        /// </summary>
        protected override void OnClosed()
        {
            _stream = null;
            base.OnClosed();
        }
        /// <summary>
        /// 数据充满管道
        /// </summary>
        /// <param name="memory">内存</param>
        /// <param name="cancellationToken">异步操作标识Token</param>
        /// <returns></returns>
        protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            return await _stream.ReadAsync(memory, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// 发送所有数据（异步）
        /// </summary>
        /// <param name="buffer">只读字节流</param>
        /// <param name="cancellationToken">异步操作标识Token</param>
        /// <returns>发送数据的长度</returns>
        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            var total = 0;

            foreach (var data in buffer)
            {
                await _stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                total += data.Length;
            }

            await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            return total;
        }
        /// <summary>
        /// 是否忽略异常
        /// </summary>
        /// <param name="e">抛出的异常</param>
        /// <returns>true:忽略；false:不忽略；</returns>
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