using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SuperSocket;
using SuperSocket.Server;

namespace JT808.Socket.Server
{
    public class MyClientService<TReceivePackage>: SuperSocketService<TReceivePackage>
    {
        public MyClientService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions) : base(serviceProvider, serverOptions)
        {
            
        }

        protected override ValueTask OnSessionConnectedAsync(IAppSession session)
        {
            return base.OnSessionConnectedAsync(session);
        }
    }
}