using System;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;

namespace JT808.Socket.Server
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var host = SuperSocketHostBuilder.Create<TextPackageInfo, LinePipelineFilter>(args)
                .ConfigureSuperSocket(options =>
                {
                    options.AddListener(new ListenOptions
                        {
                            Ip = "Any",
                            Port = 4040
                        }
                    );
                })
                .UsePackageHandler(async (s, p) =>
                {
                    Console.WriteLine(p.Text);
                    await s.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(p.Text + "\r\n")));
                })
                .Build();

            await host.RunAsync();
        }
    }
}
