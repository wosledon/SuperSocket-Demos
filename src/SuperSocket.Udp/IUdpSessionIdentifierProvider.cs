using System;
using System.Buffers;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using System.Net;

namespace SuperSocket.Udp
{
    public interface IUdpSessionIdentifierProvider
    {
        /// <summary>
        /// 获取Session标识符
        /// </summary>
        /// <param name="remoteEndPoint">远程网络标识</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        string GetSessionIdentifier(IPEndPoint remoteEndPoint, ArraySegment<byte> data);
    }
}