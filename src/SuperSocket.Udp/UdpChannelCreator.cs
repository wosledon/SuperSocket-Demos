using System;
using System.Buffers;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;

namespace SuperSocket.Udp
{
    class UdpChannelCreator : IChannelCreator
    {
        private ILogger _logger;

        private Socket _listenSocket;

        private IPEndPoint _acceptRemoteEndPoint;

        private readonly Func<Socket, IPEndPoint, string, ValueTask<IVirtualChannel>> _channelFactory;
        
        public ListenOptions Options { get;  }

        public ChannelOptions ChannelOptions { get; }

        public bool IsRunning { get; private set; }

        public event NewClientAcceptHandler NewClientAccepted;

        private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

        private CancellationTokenSource _cancellationTokenSource;

        private TaskCompletionSource<bool> _stopTaskCompletionSource;

        private IUdpSessionIdentifierProvider _udpSessionIdentifierProvider;

        private IAsyncSessionContainer _sessionContainer;
        /// <summary>
        /// Udp管道创造器
        /// </summary>
        /// <param name="options">监听设置</param>
        /// <param name="channelOptions">通道设置</param>
        /// <param name="channelFactory">通道工厂</param>
        /// <param name="logger">日志</param>
        /// <param name="udpSessionIdentifierProvider">UdpSession识别提供器</param>
        /// <param name="sessionContainer">Session容器</param>
        public UdpChannelCreator(ListenOptions options, ChannelOptions channelOptions, Func<Socket, IPEndPoint, string, ValueTask<IVirtualChannel>> channelFactory, ILogger logger, IUdpSessionIdentifierProvider udpSessionIdentifierProvider, IAsyncSessionContainer sessionContainer)
        {
            Options = options;
            ChannelOptions = channelOptions;
            _channelFactory = channelFactory;
            _logger = logger;
            _udpSessionIdentifierProvider = udpSessionIdentifierProvider;
            _sessionContainer = sessionContainer;
        }
        /// <summary>
        /// 开始
        /// </summary>
        /// <returns>true:成功; false:失败;</returns>
        public bool Start()
        {
            var options = Options;

            try
            {                
                var listenEndpoint = options.GetListenEndPoint();                
                var listenSocket = _listenSocket = new Socket(listenEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                
                if (options.NoDelay)
                    listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                
                listenSocket.Bind(listenEndpoint);

                _acceptRemoteEndPoint = listenEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? new IPEndPoint(IPAddress.IPv6Any, 0) : new IPEndPoint(IPAddress.Any, 0);

                uint IOC_IN = 0x80000000;
                uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

                byte[] optionInValue = { Convert.ToByte(false) };
                byte[] optionOutValue = new byte[4];

                try
                {
                    listenSocket.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
                }
                catch (PlatformNotSupportedException)
                {
                    _logger.LogWarning("Failed to set socket option SIO_UDP_CONNRESET because the platform doesn't support it.");
                }                

                IsRunning = true;

                _cancellationTokenSource = new CancellationTokenSource();

                KeepAccept(listenSocket).DoNotAwait();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"The listener[{this.ToString()}] failed to start.");
                return false;
            }
        }
        /// <summary>
        /// 保持应答
        /// </summary>
        /// <param name="listenSocket">监听Socket</param>
        /// <returns></returns>
        private async Task KeepAccept(Socket listenSocket)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var buffer = default(byte[]);

                try
                {
                    var bufferSize = ChannelOptions.MaxPackageLength;
                    buffer = _bufferPool.Rent(bufferSize);

                    var result = await listenSocket
                        .ReceiveFromAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), SocketFlags.None, _acceptRemoteEndPoint)
                        .ConfigureAwait(false);

                    var packageData = new ArraySegment<byte>(buffer, 0, result.ReceivedBytes);
                    var remoteEndPoint = result.RemoteEndPoint as IPEndPoint;
                    
                    var sessionID = _udpSessionIdentifierProvider.GetSessionIdentifier(remoteEndPoint, packageData);

                    var session = await _sessionContainer.GetSessionByIDAsync(sessionID);

                    IVirtualChannel channel = null;

                    if (session != null)
                    {
                        channel = session.Channel as IVirtualChannel;
                    }
                    else
                    {
                        channel = await CreateChannel(_listenSocket, remoteEndPoint, sessionID);

                        if (channel == null)
                            return;

                        OnNewClientAccept(channel);
                    }

                    await channel.WritePipeDataAsync(packageData.AsMemory(), _cancellationTokenSource.Token);
                }
                catch (Exception e)
                {
                    if (e is ObjectDisposedException || e is NullReferenceException)
                        break;
                    
                    if (e is SocketException se)
                    {
                        var errorCode = se.ErrorCode;

                        //The listen socket was closed
                        if (errorCode == 125 || errorCode == 89 || errorCode == 995 || errorCode == 10004 || errorCode == 10038)
                        {
                            break;
                        }
                    }
                    
                    _logger.LogError(e, $"Listener[{this.ToString()}] failed to receive udp data");
                }
                finally
                {
                    _bufferPool.Return(buffer);
                }
            }

            _stopTaskCompletionSource.TrySetResult(true);
        }
        /// <summary>
        /// 新客户端应答
        /// </summary>
        /// <param name="channel"></param>
        private void OnNewClientAccept(IChannel channel)
        {
            var handler = NewClientAccepted;

            if (handler == null)
                return;

            handler.Invoke(this, channel);
        }
        /// <summary>
        /// 创建通道
        /// </summary>
        /// <param name="socket">Socket</param>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="sessionIdentifier">Session标识符</param>
        /// <returns></returns>
        private async ValueTask<IVirtualChannel> CreateChannel(Socket socket, IPEndPoint remoteEndPoint, string sessionIdentifier)
        {
            try
            {
                return await _channelFactory(socket, remoteEndPoint, sessionIdentifier);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to create channel for {socket.RemoteEndPoint}.");
                return null;
            }   
        }
        /// <summary>
        /// 创建管道
        /// </summary>
        /// <param name="connection">连接</param>
        /// <returns>管道</returns>
        public async Task<IChannel> CreateChannel(object connection)
        {
            var socket = (Socket)connection;
            var remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
            return await CreateChannel(socket, remoteEndPoint, _udpSessionIdentifierProvider.GetSessionIdentifier(remoteEndPoint, null));
        }
        /// <summary>
        /// 异步停止
        /// </summary>
        /// <returns></returns>
        public Task StopAsync()
        {
            var listenSocket = _listenSocket;

            if (listenSocket == null)
                return Task.Delay(0);

            _stopTaskCompletionSource = new TaskCompletionSource<bool>();

            _cancellationTokenSource.Cancel();
            listenSocket.Close();
            
            return _stopTaskCompletionSource.Task;
        }
        /// <summary>
        /// 设置转为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Options?.ToString();
        }
    }
}