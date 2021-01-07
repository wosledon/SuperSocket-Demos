using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SuperSocket.Channel;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    interface IWebSocketCommandMiddleware : IMiddleware
    {

    }

    public class WebSocketCommandMiddleware<TKey, TPackageInfo> : CommandMiddleware<TKey, WebSocketPackage, TPackageInfo>, IWebSocketCommandMiddleware
        where TPackageInfo : class, IKeyedPackageInfo<TKey>
    {
        /// <summary>
        /// 初始化WebSocket命令中间件
        /// </summary>
        /// <param name="serviceProvider">服务提供</param>
        /// <param name="commandOptions">命令设置</param>
        public WebSocketCommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions)
            : base(serviceProvider, commandOptions)
        {
            
        }
        /// <summary>
        /// 初始化WebSocket命令中间件
        /// </summary>
        /// <param name="serviceProvider">服务提供</param>
        /// <param name="commandOptions">命令设置</param>
        /// <param name="mapper">映射器</param>
        public WebSocketCommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions, IPackageMapper<WebSocketPackage, TPackageInfo> mapper)
            : base(serviceProvider, commandOptions, mapper)
        {

        }
    }
}