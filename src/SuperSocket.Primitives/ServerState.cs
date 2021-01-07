using System;
using System.Threading.Tasks;

namespace SuperSocket
{
    /// <summary>
    /// ·þÎñÆ÷×´Ì¬
    /// </summary>
    public enum ServerState
    {
        None = 0,
        Starting = 1,
        Started = 2,
        Stopping = 3,
        Stopped = 4
    }
}