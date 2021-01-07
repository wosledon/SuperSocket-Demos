using System;
using System.IO;
using System.Net.Sockets;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client
{
    /// <summary>
    /// Socket连接状态
    /// </summary>
    public class ConnectState
    {
        public ConnectState()
        {

        }

        /// <summary>
        /// 初始化连接状态
        /// </summary>
        /// <param name="cancelled">通道状态：打开/关闭</param>
        private ConnectState(bool cancelled)
        {
            Cancelled = cancelled;
        }

        public bool Result { get; set; }

        public bool Cancelled { get; private set; }

        public Exception Exception { get; set; }

        public Socket Socket { get; set; }

        public Stream Stream { get; set; }

        /// <summary>
        /// 关闭通道
        /// </summary>
        public static readonly ConnectState CancelledState = new ConnectState(false);

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <typeparam name="TReceivePackage">接收包</typeparam>
        /// <param name="pipelineFilter">内置命令行</param>
        /// <param name="channelOptions">通道设置</param>
        /// <returns>Stream/Tcp通道</returns>
        public IChannel<TReceivePackage> CreateChannel<TReceivePackage>(IPipelineFilter<TReceivePackage> pipelineFilter, ChannelOptions channelOptions)
            where TReceivePackage : class
        {
            var stream = this.Stream;
            var socket = this.Socket;

            if (stream != null)
            {
                return new StreamPipeChannel<TReceivePackage>(stream , socket.RemoteEndPoint, socket.LocalEndPoint, pipelineFilter, channelOptions);
            }
            else
            {
                return new TcpPipeChannel<TReceivePackage>(socket, pipelineFilter, channelOptions);
            }
        }
    }
}