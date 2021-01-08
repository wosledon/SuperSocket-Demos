using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Models
{
    public enum OpCode
    {
        Connect = 1,
        DisConnect = 2,
        Subscribe = 3,
        Single = 4,
        All = 5
    }
}
