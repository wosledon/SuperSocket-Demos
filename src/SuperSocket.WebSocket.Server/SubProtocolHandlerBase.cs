using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    abstract class SubProtocolHandlerBase : ISubProtocolHandler
    {
        public string Name { get; private set; }
        /// <summary>
        /// 初始化子协议处理基类
        /// </summary>
        /// <param name="name">名称</param>
        public SubProtocolHandlerBase(string name)
        {
            Name = name;
        }        
        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="package">WebSocket包</param>
        /// <returns></returns>
        public abstract ValueTask Handle(IAppSession session, WebSocketPackage package);
    }
}