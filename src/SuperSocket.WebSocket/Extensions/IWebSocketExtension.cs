using System;
using System.Buffers;

namespace SuperSocket.WebSocket.Extensions
{
    /// <summary>
    /// WebSocket Extensions
    /// https://tools.ietf.org/html/rfc6455#section-9
    /// </summary>
    public interface IWebSocketExtension
    {
        /// <summary>
        /// Ãû³Æ
        /// </summary>
        string Name { get; }
        /// <summary>
        /// ±àÂë
        /// </summary>
        /// <param name="package"></param>
        void Encode(WebSocketPackage package);
        /// <summary>
        /// ½âÂë
        /// </summary>
        /// <param name="package"></param>
        void Decode(WebSocketPackage package);
    }
}
