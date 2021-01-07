using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace SuperSocket
{
    public static class SuperSocketExtensions
    {
        /// <summary>
        /// 保活连接
        /// </summary>
        /// <param name="server">服务器</param>
        /// <param name="remoteEndpoint">远程网络标识</param>
        /// <returns></returns>
        public static async ValueTask<bool> ActiveConnect(this IServer server, EndPoint remoteEndpoint)
        {
            var socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                await socket.ConnectAsync(remoteEndpoint);
                await (server as IChannelRegister).RegisterChannel(socket);
                return true;
            }
            catch (Exception e)
            {
                var loggerFactory = server.ServiceProvider.GetService<ILoggerFactory>();

                if (loggerFactory != null)
                    loggerFactory.CreateLogger(nameof(ActiveConnect)).LogError(e, $"Failed to connect to {remoteEndpoint}");

                return false;
            }
        }
        /// <summary>
        /// 获取默认日志收集器
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static ILogger GetDefaultLogger(this IAppSession session)
        {
            return (session.Server as ILoggerAccessor)?.Logger;
        }
        /// <summary>
        /// 添加监听
        /// </summary>
        /// <param name="serverOptions">服务器设置</param>
        /// <param name="listener">监听设置</param>
        /// <returns></returns>
        public static ServerOptions AddListener(this ServerOptions serverOptions, ListenOptions listener)
        {
            var listeners = serverOptions.Listeners;

            if (listeners == null)
                listeners = serverOptions.Listeners = new List<ListenOptions>();

            listeners.Add(listener);
            return serverOptions;
        }
    }
}
