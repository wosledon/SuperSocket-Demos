using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.WebSocket.Server;

namespace JT808.WebSocket.Server
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var host = WebSocketHostBuilder.Create()
                .UseWebSocketMessageHandler(
                    async (session, message) =>
                    {
                        Console.WriteLine(message.Message);
                        await session.SendAsync(message.Message);
                    }
                )
                .ConfigureAppConfiguration((hostCtx, configApp) =>
                {
                    configApp.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "serverOptions:name", "JT808.Server" },
                        { "serverOptions:listeners:0:ip", "Any" },
                        { "serverOptions:listeners:0:port", "4040" }
                    });
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
