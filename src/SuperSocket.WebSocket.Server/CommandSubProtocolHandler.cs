using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    sealed class CommandSubProtocolHandler<TPackageInfo> : SubProtocolHandlerBase
    {
        private IPackageHandler<WebSocketPackage> _commandMiddleware;
        /// <summary>
        /// 初始化命令子协议处理器
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="serviceProvider">服务提供</param>
        /// <param name="commandOptions">命令设置</param>
        /// <param name="mapper">映射器</param>
        public CommandSubProtocolHandler(string name, IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions, IPackageMapper<WebSocketPackage, TPackageInfo> mapper)
            : base(name)
        {
            var keyType = CommandMiddlewareExtensions.GetKeyType<TPackageInfo>();
            var commandMiddlewareType = typeof(WebSocketCommandMiddleware<,>).MakeGenericType(keyType, typeof(TPackageInfo));
            _commandMiddleware = Activator.CreateInstance(commandMiddlewareType, serviceProvider, commandOptions, mapper) as IPackageHandler<WebSocketPackage>;
        }
        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="package">WebSocket包</param>
        /// <returns></returns>
        public override async ValueTask Handle(IAppSession session, WebSocketPackage package)
        {
            await _commandMiddleware.Handle(session, package);
        }
    }
}