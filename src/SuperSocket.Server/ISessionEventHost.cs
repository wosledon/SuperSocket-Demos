using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    public interface ISessionEventHost
    {
        /// <summary>
        /// 处理Session连接事件
        /// </summary>
        /// <param name="session">Session</param>
        /// <returns></returns>
        ValueTask HandleSessionConnectedEvent(AppSession session);
        /// <summary>
        /// 处理Session关闭事件
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="reason">关闭原因</param>
        /// <returns></returns>
        ValueTask HandleSessionClosedEvent(AppSession session, CloseReason reason);
    }
}