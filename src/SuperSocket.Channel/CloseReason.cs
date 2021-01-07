using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    /// <summary>
    /// 通道/管道关闭原因枚举
    /// </summary>
    public enum CloseReason
    {
        /// <summary>
        /// The socket is closed for unknown reason
        /// 由于未知错误导致Socket关闭
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Close for server shutdown
        /// 由于服务器关机导致Socket关闭
        /// </summary>
        ServerShutdown = 1,

        /// <summary>
        /// The close behavior is initiated from the remote endpoing
        /// 关闭的行为由远程节点发起
        /// </summary>
        RemoteClosing = 2,

        /// <summary>
        /// The close behavior is initiated from the local endpoing
        /// 关闭行为由本地节点发起
        /// </summary>
        LocalClosing = 3,

        /// <summary>
        /// Application error
        /// 应用错误
        /// </summary>
        ApplicationError = 4,

        /// <summary>
        /// The socket is closed for a socket error
        /// Socket错误
        /// </summary>
        SocketError = 5,

        /// <summary>
        /// The socket is closed by server for timeout
        /// 由于服务器超时导致Soctet关闭
        /// </summary>
        TimeOut = 6,

        /// <summary>
        /// Protocol error 
        /// 协议错误
        /// </summary>
        ProtocolError = 7,

        /// <summary>
        /// SuperSocket internal error
        /// 内部错误
        /// </summary>
        InternalError = 8,
    }

    public class CloseEventArgs : EventArgs
    {
        public CloseReason Reason { get; private set; }

        public CloseEventArgs(CloseReason reason)
        {
            Reason = reason;
        }
    }
}