using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    public class CloseStatus
    {
        /// <summary>
        /// 关闭原因
        /// </summary>
        public CloseReason Reason { get; set; }
        /// <summary>
        /// 原因说明
        /// </summary>
        public string ReasonText { get; set; }
        /// <summary>
        /// 远程启动
        /// </summary>
        public bool RemoteInitiated{ get; set; }
    }
}
