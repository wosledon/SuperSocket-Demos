using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Channel
{
    public static class SocketExtensions
    {
        /// <summary>
        /// ºöÂÔSocketµÄ´íÎó´úÂë
        /// </summary>
        /// <param name="se">´íÎóĞÅÏ¢</param>
        /// <returns>true:ºöÂÔ£»false:²»ºöÂÔ£»</returns>
        internal static bool IsIgnorableSocketException(this SocketException se)
        {
            if (se.ErrorCode == 89)
                return true;

            if (se.ErrorCode == 125)
                return true;

            if (se.ErrorCode == 104)
                return true;

            if (se.ErrorCode == 54)
                return true;

            if (se.ErrorCode == 10054)
                return true;

            if (se.ErrorCode == 995)
                return true;

            return false;
        }
    }
}
