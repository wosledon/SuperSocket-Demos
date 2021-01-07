using System;
using System.Buffers;
using System.Collections.Specialized;
using SuperSocket.WebSocket.Extensions;

namespace SuperSocket.WebSocket.Server.Extensions
{

    public interface IWebSocketExtensionFactory
    {
        string Name { get; }
        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="options">设置</param>
        /// <param name="supportedOptions">支持的设置</param>
        /// <returns></returns>
        IWebSocketExtension Create(NameValueCollection options, out NameValueCollection supportedOptions);
    }
}
