using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    public class SessionHandlers
    {
        /// <summary>
        /// Á¬½Ó
        /// </summary>
        public Func<IAppSession, ValueTask> Connected { get; set; }
        /// <summary>
        /// ¹Ø±Õ
        /// </summary>
        public Func<IAppSession, CloseEventArgs, ValueTask> Closed { get; set; }
    }
}