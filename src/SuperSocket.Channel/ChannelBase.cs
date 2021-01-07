using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public abstract class ChannelBase<TPackageInfo> : IChannel<TPackageInfo>, IChannel
    {
        /// <summary>
        /// 打开通道
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// 运行（异步）
        /// </summary>
        /// <returns>数据包信息</returns>
        public abstract IAsyncEnumerable<TPackageInfo> RunAsync();
        /// <summary>
        /// 发送数据（异步）
        /// </summary>
        /// <param name="buffer">发送的buffer数据</param>
        /// <returns></returns>
        public abstract ValueTask SendAsync(ReadOnlyMemory<byte> buffer);
        /// <summary>
        /// 发送数据（异步）
        /// </summary>
        /// <typeparam name="TPackage">包实体</typeparam>
        /// <param name="packageEncoder">包编码</param>
        /// <param name="package">数据包</param>
        /// <returns></returns>
        public abstract ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);
        /// <summary>
        /// 发送数据（异步）
        /// </summary>
        /// <param name="write">写入数据</param>
        /// <returns></returns>
        public abstract ValueTask SendAsync(Action<PipeWriter> write);
        /// <summary>
        /// 管道是否关闭
        /// </summary>
        public bool IsClosed { get; private set; }
        /// <summary>
        /// 远程网络标识
        /// </summary>
        public EndPoint RemoteEndPoint { get; protected set; }
        /// <summary>
        /// 本地网络标识
        /// </summary>
        public EndPoint LocalEndPoint { get; protected set; }
        /// <summary>
        /// 关闭原因
        /// </summary>
        public CloseReason? CloseReason { get; protected set; }
        /// <summary>
        /// 最后一次活动时间
        /// </summary>
        public DateTimeOffset LastActiveTime { get; protected set; } = DateTimeOffset.Now;
        /// <summary>
        /// 关闭管道
        /// </summary>
        protected virtual void OnClosed()
        {
            IsClosed = true;

            var closed = Closed;

            if (closed == null)
                return;

            if (Interlocked.CompareExchange(ref Closed, null, closed) != closed)
                return;

            var closeReason = CloseReason.HasValue ? CloseReason.Value : Channel.CloseReason.Unknown;

            closed.Invoke(this, new CloseEventArgs(closeReason));
        }
        /// <summary>
        /// 关闭事件
        /// </summary>
        public event EventHandler<CloseEventArgs> Closed;
        /// <summary>
        /// 关闭通道（异步）
        /// </summary>
        /// <param name="closeReason"></param>
        /// <returns></returns>
        public abstract ValueTask CloseAsync(CloseReason closeReason);
        /// <summary>
        /// 分离（异步）
        /// </summary>
        /// <returns></returns>
        public abstract ValueTask DetachAsync();
    }
}
