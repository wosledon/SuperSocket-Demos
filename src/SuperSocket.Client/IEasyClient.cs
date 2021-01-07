using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client
{
    public interface IEasyClient<TReceivePackage, TSendPackage> : IEasyClient<TReceivePackage>
        where TReceivePackage : class
    {
        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="package">数据包</param>
        /// <returns></returns>
        ValueTask SendAsync(TSendPackage package);      
    }

    
    public interface IEasyClient<TReceivePackage>
        where TReceivePackage : class
    {
        /// <summary>
        /// 异步连接
        /// </summary>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="cancellationToken">异步操作Token</param>
        /// <returns>true:成功；false:失败；</returns>
        ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken = default);
        /// <summary>
        /// 异步接收
        /// </summary>
        /// <returns></returns>
        ValueTask<TReceivePackage> ReceiveAsync();
        /// <summary>
        /// 本地网络标识
        /// </summary>
        IPEndPoint LocalEndPoint { get; set; }
        /// <summary>
        /// 安全选项
        /// </summary>
        SecurityOptions Security { get; set; }
        /// <summary>
        /// 开始接收
        /// </summary>
        void StartReceive();
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        ValueTask SendAsync(ReadOnlyMemory<byte> data);
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <typeparam name="TSendPackage">数据包的类型</typeparam>
        /// <param name="packageEncoder">包的编码</param>
        /// <param name="package">数据包</param>
        /// <returns></returns>
        ValueTask SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package);

        event EventHandler Closed;

        event PackageHandler<TReceivePackage> PackageHandler;
        /// <summary>
        /// 异步关闭
        /// </summary>
        /// <returns></returns>
        ValueTask CloseAsync();
    }
}