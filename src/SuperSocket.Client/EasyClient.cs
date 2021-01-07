using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;


namespace SuperSocket.Client
{
    public class EasyClient<TPackage, TSendPackage> : EasyClient<TPackage>, IEasyClient<TPackage, TSendPackage>
        where TPackage : class
    {
        private IPackageEncoder<TSendPackage> _packageEncoder;
        /// <summary>
        /// 初始化发送客户端
        /// </summary>
        /// <param name="packageEncoder">包编码格式</param>
        protected EasyClient(IPackageEncoder<TSendPackage> packageEncoder)
            : base()
        {
            _packageEncoder = packageEncoder;
        }
        /// <summary>
        /// 初始化发送客户端
        /// </summary>
        /// <param name="pipelineFilter">管道筛选器</param>
        /// <param name="packageEncoder">包编码</param>
        /// <param name="logger">日志</param>
        public EasyClient(IPipelineFilter<TPackage> pipelineFilter, IPackageEncoder<TSendPackage> packageEncoder, ILogger logger = null)
            : this(pipelineFilter, packageEncoder, new ChannelOptions { Logger = logger })
        {

        }
        /// <summary>
        /// 初始化发送客户端
        /// </summary>
        /// <param name="pipelineFilter">管道筛选器</param>
        /// <param name="packageEncoder">包编码</param>
        /// <param name="options">通道选项</param>
        public EasyClient(IPipelineFilter<TPackage> pipelineFilter, IPackageEncoder<TSendPackage> packageEncoder, ChannelOptions options)
            : base(pipelineFilter, options)
        {
            _packageEncoder = packageEncoder;
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public virtual async ValueTask SendAsync(TSendPackage package)
        {
            await SendAsync(_packageEncoder, package);
        }
        /// <summary>
        /// 作为发送客户端
        /// </summary>
        /// <returns>当前实例</returns>
        public new IEasyClient<TPackage, TSendPackage> AsClient()
        {
            return this;
        }
    }

    public class EasyClient<TReceivePackage> : IEasyClient<TReceivePackage>
        where TReceivePackage : class
    {
        private IPipelineFilter<TReceivePackage> _pipelineFilter;

        protected IChannel<TReceivePackage> Channel { get; private set; }

        protected ILogger Logger { get; set; }

        protected ChannelOptions Options { get; private set; }

        IAsyncEnumerator<TReceivePackage> _packageStream;

        public event PackageHandler<TReceivePackage> PackageHandler;

        public IPEndPoint LocalEndPoint { get; set; }

        public SecurityOptions Security { get; set; }

        protected EasyClient()
        {

        }
        /// <summary>
        /// 初始化接收客户端
        /// </summary>
        /// <param name="pipelineFilter">管道筛选器</param>
        public EasyClient(IPipelineFilter<TReceivePackage> pipelineFilter)
            : this(pipelineFilter, NullLogger.Instance)
        {
            
        }
        /// <summary>
        /// 初始化接收客户端
        /// </summary>
        /// <param name="pipelineFilter">管道筛选器</param>
        /// <param name="logger">日志</param>
        public EasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, ILogger logger)
            : this(pipelineFilter, new ChannelOptions { Logger = logger })
        {

        }
        /// <summary>
        /// 初始化接收客户端
        /// </summary>
        /// <param name="pipelineFilter">管道筛选器</param>
        /// <param name="options">管道选项</param>
        public EasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, ChannelOptions options)
        {
            if (pipelineFilter == null)
                throw new ArgumentNullException(nameof(pipelineFilter));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _pipelineFilter = pipelineFilter;
            Options = options;
            Logger = options.Logger;
        }
        /// <summary>
        /// 作为接收客户端
        /// </summary>
        /// <returns>当前实例</returns>
        public virtual IEasyClient<TReceivePackage> AsClient()
        {
            return this;
        }
        /// <summary>
        /// 获取连接器
        /// </summary>
        /// <returns>连接器实例</returns>
        protected virtual IConnector GetConnector()
        {
            var security = Security;

            if (security != null)
            {
                if (security.EnabledSslProtocols != SslProtocols.None)
                    return new SocketConnector(LocalEndPoint, new SslStreamConnector(security));
            }
            
            return new SocketConnector(LocalEndPoint);
        }
        /// <summary>
        /// 异步连接
        /// </summary>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="cancellationToken">异步操作标识Token</param>
        /// <returns></returns>
        ValueTask<bool> IEasyClient<TReceivePackage>.ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            return ConnectAsync(remoteEndPoint, cancellationToken);
        }
        /// <summary>
        /// 异步连接
        /// </summary>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="cancellationToken">异步操作Token</param>
        /// <returns></returns>
        protected virtual async ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            var connector = GetConnector();
            var state = await connector.ConnectAsync(remoteEndPoint, null, cancellationToken);

            if (state.Cancelled || cancellationToken.IsCancellationRequested)
            {
                OnError($"The connection to {remoteEndPoint} was cancelled.", state.Exception);
                return false;
            }                

            if (!state.Result)
            {
                OnError($"Failed to connect to {remoteEndPoint}", state.Exception);
                return false;
            }

            var socket = state.Socket;

            if (socket == null)
                throw new Exception("Socket is null.");

            var channelOptions = Options;
            SetupChannel(state.CreateChannel<TReceivePackage>(_pipelineFilter, channelOptions));
            return true;
        }
        /// <summary>
        /// 作为Udp
        /// </summary>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="bufferPool">buffer池</param>
        /// <param name="bufferSize">buffer的大小</param>
        public void AsUdp(IPEndPoint remoteEndPoint, ArrayPool<byte> bufferPool = null, int bufferSize = 4096)
        { 
            var localEndPoint = LocalEndPoint;

            if (localEndPoint == null)
            {
                localEndPoint = new IPEndPoint(remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any, 0);
            }

            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            
            // bind the local endpoint
            socket.Bind(localEndPoint);

            var channel = new UdpPipeChannel<TReceivePackage>(socket, _pipelineFilter, this.Options, remoteEndPoint);

            SetupChannel(channel);

            UdpReceive(socket, channel, bufferPool, bufferSize);
        }
        /// <summary>
        /// Udp接收
        /// </summary>
        /// <param name="socket">Socket实例</param>
        /// <param name="channel">通道</param>
        /// <param name="bufferPool">buffer池</param>
        /// <param name="bufferSize">buffer大小</param>
        private async void UdpReceive(Socket socket, UdpPipeChannel<TReceivePackage> channel, ArrayPool<byte> bufferPool, int bufferSize)
        {
            if (bufferPool == null)
                bufferPool = ArrayPool<byte>.Shared;

            while (true)
            {
                var buffer = bufferPool.Rent(bufferSize);

                try
                {
                    var result = await socket
                        .ReceiveFromAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), SocketFlags.None, channel.RemoteEndPoint)
                        .ConfigureAwait(false);

                    await channel.WritePipeDataAsync((new ArraySegment<byte>(buffer, 0, result.ReceivedBytes)).AsMemory(), CancellationToken.None);
                }
                catch (NullReferenceException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception e)
                {
                    OnError($"Failed to receive UDP data.", e);
                }
                finally
                {
                    bufferPool.Return(buffer);
                }
            }
        }
        /// <summary>
        /// 设置管道
        /// </summary>
        /// <param name="channel"></param>
        protected virtual void SetupChannel(IChannel<TReceivePackage> channel)
        {
            channel.Closed += OnChannelClosed;
            channel.Start();
            _packageStream = channel.GetPackageStream();
            Channel = channel;
        }
        /// <summary>
        /// 异步接收
        /// </summary>
        /// <returns></returns>
        ValueTask<TReceivePackage> IEasyClient<TReceivePackage>.ReceiveAsync()
        {
            return ReceiveAsync();
        }

        /// <summary>
        /// Try to receive one package
        /// 异步接收
        /// </summary>
        /// <returns></returns>
        protected virtual async ValueTask<TReceivePackage> ReceiveAsync()
        {
            var p = await _packageStream.ReceiveAsync();

            if (p != null)
                return p;

            OnClosed(Channel, EventArgs.Empty);
            return null;
        }
        /// <summary>
        /// 开始接收
        /// </summary>
        void IEasyClient<TReceivePackage>.StartReceive()
        {
            StartReceive();
        }

        /// <summary>
        /// Start receive packages and handle the packages by event handler
        /// 开始接收并处理数据包
        /// </summary>
        protected virtual void StartReceive()
        {
            StartReceiveAsync();
        }
        /// <summary>
        /// 开始异步接收
        /// </summary>
        private async void StartReceiveAsync()
        {
            var enumerator = _packageStream;

            while (await enumerator.MoveNextAsync())
            {
                await OnPackageReceived(enumerator.Current);
            }
        }
        /// <summary>
        /// 数据包接收时
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        protected virtual async ValueTask OnPackageReceived(TReceivePackage package)
        {
            var handler = PackageHandler;

            try
            {
                await handler.Invoke(this, package);
            }
            catch (Exception e)
            {
                OnError("Unhandled exception happened in PackageHandler.", e);
            }
        }
        /// <summary>
        /// 通道关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChannelClosed(object sender, EventArgs e)
        {
            Channel.Closed -= OnChannelClosed;
            OnClosed(this, e);
        }
        /// <summary>
        /// 关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClosed(object sender, EventArgs e)
        {
            var handler = Closed;

            if (handler != null)
            {
                if (Interlocked.CompareExchange(ref Closed, null, handler) == handler)
                {
                    handler.Invoke(sender, e);
                }
            }
        }
        /// <summary>
        /// 发生错误时
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="exception">错误信息</param>
        protected virtual void OnError(string message, Exception exception)
        {
            Logger?.LogError(exception, message);
        }
        /// <summary>
        /// 发生错误时
        /// </summary>
        /// <param name="message">信息</param>
        protected virtual void OnError(string message)
        {
            Logger?.LogError(message);
        }
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        ValueTask IEasyClient<TReceivePackage>.SendAsync(ReadOnlyMemory<byte> data)
        {
            return SendAsync(data);
        }
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        protected virtual async ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            await Channel.SendAsync(data);
        }
        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <typeparam name="TSendPackage">发送包的类型</typeparam>
        /// <param name="packageEncoder">包编码</param>
        /// <param name="package">数据包</param>
        /// <returns></returns>
        ValueTask IEasyClient<TReceivePackage>.SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package)
        {
            return SendAsync<TSendPackage>(packageEncoder, package);
        }
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <typeparam name="TSendPackage">发送包的类型</typeparam>
        /// <param name="packageEncoder">包编码</param>
        /// <param name="package">数据包</param>
        /// <returns></returns>
        protected virtual async ValueTask SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package)
        {
            await Channel.SendAsync(packageEncoder, package);
        }

        public event EventHandler Closed;
        /// <summary>
        /// 异步关闭通道
        /// </summary>
        /// <returns></returns>
        public virtual async ValueTask CloseAsync()
        {
            await Channel.CloseAsync(CloseReason.LocalClosing);
            OnClosed(this, EventArgs.Empty);
        }
    }
}
