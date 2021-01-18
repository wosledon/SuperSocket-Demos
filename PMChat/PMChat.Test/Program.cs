using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using PMChat.Models;
using SuperSocket.Channel;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace PMChat.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Thread[] threads = new Thread[500];
            for (int i = 0; i < 500; i++)
            {
                threads[i] = new Thread(CreateClient);
                threads[i].Start();

                Console.WriteLine($"第[{i+1}]个客户端已启动");
                Task.Delay(200).Wait();
            }

            Console.ReadKey();
        }

        static async void CreateClient()
        {
            var options = new ChannelOptions
            {
                Logger = NullLogger.Instance,
                ReadAsDemand = true
            };

            var client = new EasyClient<TextPackageInfo>(new LinePipelineFilter(), options).AsClient();

            var connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8888));

            var connectPackage = new TcpPackage()
            {
                OpCode = OpCode.Connect,
                LocalName = new Guid().ToString(),
                RemoteName = "Server",
                MessageType = MessageType.Text
            };

            await client.SendAsync(
                new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(connectPackage.ToString())));

            while (true)
            {
                var msgPackage = new TcpPackage()
                {
                    OpCode = OpCode.Connect,
                    LocalName = new Guid().ToString(),
                    RemoteName = "All",
                    MessageType = MessageType.Text
                };

                await client.SendAsync(
                    new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(msgPackage.ToString())));
                Task.Delay(500).Wait();
            }
        }
    }
}
