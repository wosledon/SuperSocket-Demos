using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket
{
    public interface IAppSession
    {
        /// <summary>
        /// SessionID
        /// </summary>
        string SessionID { get; }
        /// <summary>
        /// 开始时间
        /// </summary>
        DateTimeOffset StartTime { get; }
        /// <summary>
        /// 最后一次活动时间
        /// </summary>
        DateTimeOffset LastActiveTime { get; }
        /// <summary>
        /// 通道
        /// </summary>
        IChannel Channel { get; }
        /// <summary>
        /// 远程网络标识
        /// </summary>
        EndPoint RemoteEndPoint { get; }
        /// <summary>
        /// 本地网络标识
        /// </summary>
        EndPoint LocalEndPoint { get; }
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        ValueTask SendAsync(ReadOnlyMemory<byte> data);
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <typeparam name="TPackage">包类型</typeparam>
        /// <param name="packageEncoder">包编码</param>
        /// <param name="package">数据包</param>
        /// <returns></returns>
        ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);
        /// <summary>
        /// 异步关闭
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        ValueTask CloseAsync(CloseReason reason);
        /// <summary>
        /// 服务器
        /// </summary>
        IServerInfo Server { get; }
        /// <summary>
        /// 异步连接处理
        /// </summary>
        event AsyncEventHandler Connected;
        /// <summary>
        /// 异步关闭处理
        /// </summary>
        event AsyncEventHandler<CloseEventArgs> Closed;
        /// <summary>
        /// 数据上下文
        /// </summary>
        object DataContext { get; set; }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="server">服务器</param>
        /// <param name="channel">通道</param>
        void Initialize(IServerInfo server, IChannel channel);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object this[object name] { get; set; }
        /// <summary>
        /// Session状态
        /// </summary>
        SessionState State { get; }
        /// <summary>
        /// 重置
        /// </summary>
        void Reset();
    }
}