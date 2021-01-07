using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    class DelegateSubProtocolHandler : SubProtocolHandlerBase
    {
        private Func<WebSocketSession, WebSocketPackage, ValueTask> _packageHandler;
        /// <summary>
        /// 初始化子协议处理委托
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="packageHandler">包处理器</param>
        public DelegateSubProtocolHandler(string name, Func<WebSocketSession, WebSocketPackage, ValueTask> packageHandler)
            : base(name)
        {
            _packageHandler = packageHandler;
        }
        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="package">WebSocket包</param>
        /// <returns></returns>
        public override async ValueTask Handle(IAppSession session, WebSocketPackage package)
        {
            await _packageHandler(session as WebSocketSession, package);
        }
    }
}