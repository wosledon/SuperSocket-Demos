using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public interface IChannel
    {
        /// <summary>
        /// 打开
        /// </summary>
        void Start();
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        ValueTask SendAsync(ReadOnlyMemory<byte> data);
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <typeparam name="TPackage"></typeparam>
        /// <param name="packageEncoder">包编码格式</param>
        /// <param name="package">数据包</param>
        /// <returns></returns>
        ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="write">通道写入器</param>
        /// <returns></returns>
        ValueTask SendAsync(Action<PipeWriter> write);
        /// <summary>
        /// 异步关闭
        /// </summary>
        /// <param name="closeReason"></param>
        /// <returns></returns>
        ValueTask CloseAsync(CloseReason closeReason);
        /// <summary>
        /// 关闭事件
        /// </summary>
        event EventHandler<CloseEventArgs> Closed;
        /// <summary>
        /// 关闭状态
        /// </summary>
        bool IsClosed { get; }
        /// <summary>
        /// 远程网络标识
        /// </summary>
        EndPoint RemoteEndPoint { get; }
        /// <summary>
        /// 本地网络标识
        /// </summary>
        EndPoint LocalEndPoint { get; }
        /// <summary>
        /// 最后一次活动时间
        /// </summary>
        DateTimeOffset LastActiveTime { get; }
        /// <summary>
        /// 异步分离
        /// </summary>
        /// <returns></returns>
        ValueTask DetachAsync();
        /// <summary>
        /// 关闭原因
        /// </summary>
        CloseReason? CloseReason { get; }
    }

    public interface IChannel<TPackageInfo> : IChannel
    {
        /// <summary>
        /// 异步运行
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerable<TPackageInfo> RunAsync();
    }
}
