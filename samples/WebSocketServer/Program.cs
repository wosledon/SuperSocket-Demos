using System.Threading.Tasks;
using SuperSocket.WebSocket.Server;

namespace WebSocketServer
{
    class Program
    {
        static async Task Main(string[] args)
        {               
            var host = WebSocketHostBuilder.Create(args)
                .UseWebSocketMessageHandler(async (session, message) =>
                {
                    // echo message back to the client
                    await session.SendAsync(message.Message);
                })
                .UsePerMessageCompression()
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    // register your logging library here
                    loggingBuilder.AddConsole();
                }).Build();

            await host.RunAsync();
        }
    }
}
