using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SuperSocket;
using SuperSocket.Channel;
using SuperSocket.Server;

namespace PMChat.WebServer
{
    public class ChatService<TReceivePackageInfo>: SuperSocketService<TReceivePackageInfo>
        where TReceivePackageInfo:class
    {
        public ChatService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions) 
            : base(serviceProvider, serverOptions)
        {
        }

        protected override ValueTask OnSessionConnectedAsync(IAppSession session)
        {
            return base.OnSessionConnectedAsync(session);
        }

        protected override ValueTask OnSessionClosedAsync(IAppSession session, CloseEventArgs e)
        {
            return base.OnSessionClosedAsync(session, e);
        }

        protected override ValueTask OnStartedAsync()
        {
            return base.OnStartedAsync();
        }

        protected override ValueTask OnStopAsync()
        {
            return base.OnStopAsync();
        }
    }
}