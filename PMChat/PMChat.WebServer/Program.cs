using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PMChat.Models;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.SessionContainer;

namespace PMChat.WebServer
{
    public class Program
    {
        private static List<ClientInfo> _clients = new List<ClientInfo>();
        private static ISessionContainer _sessionContainer;
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .AsSuperSocketHostBuilder<TextPackageInfo, LinePipelineFilter>()
                .UseSession<MySession>()
                .UseHostedService<ChatService<TextPackageInfo>>()
                .UseSessionHandler(async s =>
                {
                    var data = new TcpPackage()
                    {
                        OpCode = OpCode.Connect,
                        LocalName = "Server",
                        RemoteName = "All",
                        MessageType = MessageType.Text,
                        Message = null,
                        Clients = _clients
                    };
                    //var sessions = _sessionContainer.GetSessions();
                })
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Encoding.UTF8.GetBytes(p.Text + "\r\n"));
                })
                .UseMiddleware<InProcSessionContainerMiddleware>()
                .UseInProcSessionContainer()
                .Build();

            host.StartAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args);
    }
}
