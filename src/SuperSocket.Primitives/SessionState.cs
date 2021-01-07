using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    /// <summary>
    /// Session状态
    /// </summary>
    public enum SessionState
    {
        None = 0,

        Initialized = 1,

        Connected = 2,

        Closed = 3
    }
}