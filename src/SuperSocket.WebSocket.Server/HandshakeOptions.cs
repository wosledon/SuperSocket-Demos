using System;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket.Server
{
    public class HandshakeOptions
    {
        /// <summary>
        /// Handshake queue checking interval, in seconds
        /// 握手队列检查间隔，以秒为单位
        /// </summary>
        /// <value>default: 60</value>
        public int CheckingInterval { get; set; } = 60;

        /// <summary>
        /// Open handshake timeout, in seconds
        /// 打开握手超时，以秒为单位
        /// </summary>
        /// <value>default: 120</value>
        public int OpenHandshakeTimeOut { get; set; } = 120;

        /// <summary>
        /// Close handshake timeout, in seconds
        /// 关闭握手超时，以秒为单位
        /// </summary>
        /// <value>default: 120</value>
        public int CloseHandshakeTimeOut { get; set; } = 120;

        /// <summary>
        /// 握手验证器
        /// </summary>
        public Func<WebSocketSession, WebSocketPackage, ValueTask<bool>> HandshakeValidator { get; set; }
    }
}