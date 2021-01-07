using System;
using System.Buffers;
using System.Collections.Generic;
using SuperSocket.WebSocket.Extensions;

namespace SuperSocket.WebSocket
{
    public class WebSocketPipelineFilterContext
    {
        /// <summary>
        /// À©Õ¹ÁÐ±í
        /// </summary>
        public IReadOnlyList<IWebSocketExtension> Extensions { get; set; }
    }
}
