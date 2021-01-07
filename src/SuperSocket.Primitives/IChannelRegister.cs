
using System;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;
using SuperSocket.Channel;
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IChannelRegister
    {
        /// <summary>
        /// 注册通道
        /// </summary>
        /// <param name="connection">连接</param>
        /// <returns></returns>
        Task RegisterChannel(object connection);
    }
}