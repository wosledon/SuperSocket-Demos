using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMChat.Models
{
    public enum OpCode: byte
    {
        Connect = 0,
        DisConnect = 1,
        Single = 2,
        All = 3,
        ACK = 4
    }
}
