using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.SessionContainer;

namespace PMChat.WebServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .AsSuperSocketHostBuilder<TextPackageInfo, LinePipelineFilter>()
                .UseSession<MySession>()
                .UseHostedService<ChatService<TextPackageInfo>>()
                .UseSessionHandler(s =>
                {
                    return default;
                })
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Encoding.UTF8.GetBytes(p.Text + "\r\n"));
                })
                .UseMiddleware<InProcSessionContainerMiddleware>()
                .UseInProcSessionContainer();
    }
}
