using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Udp
{
    class UdpChannelCreatorFactory : IChannelCreatorFactory
    {
        private readonly IUdpSessionIdentifierProvider _udpSessionIdentifierProvider;

        private readonly IAsyncSessionContainer _sessionContainer;
        /// <summary>
        /// 初始化Udp管道创造器工厂
        /// </summary>
        /// <param name="udpSessionIdentifierProvider">UdpSession标识符供应</param>
        /// <param name="sessionContainer">Session容器</param>
        public UdpChannelCreatorFactory(IUdpSessionIdentifierProvider udpSessionIdentifierProvider, IAsyncSessionContainer sessionContainer)
        {
            _udpSessionIdentifierProvider = udpSessionIdentifierProvider;
            _sessionContainer = sessionContainer;
        }
        /// <summary>
        /// 创建通道创造器
        /// </summary>
        /// <typeparam name="TPackageInfo">包信息的类型</typeparam>
        /// <param name="options">监听设置</param>
        /// <param name="channelOptions">通道设置</param>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="pipelineFilterFactory">管道筛选工厂</param>
        /// <returns>通道创造器</returns>
        public IChannelCreator CreateChannelCreator<TPackageInfo>(ListenOptions options, ChannelOptions channelOptions, ILoggerFactory loggerFactory, object pipelineFilterFactory)
        {
            var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;
            channelOptions.Logger = loggerFactory.CreateLogger(nameof(IChannel));
            var channelFactoryLogger = loggerFactory.CreateLogger(nameof(UdpChannelCreator));

            var channelFactory = new Func<Socket, IPEndPoint, string, ValueTask<IVirtualChannel>>((s, re, id) =>
            {
                var filter = filterFactory.Create(s);
                return new ValueTask<IVirtualChannel>(new UdpPipeChannel<TPackageInfo>(s, filter, channelOptions, re, id));
            });

            return new UdpChannelCreator(options, channelOptions, channelFactory, channelFactoryLogger, _udpSessionIdentifierProvider, _sessionContainer);
        }
    }
}