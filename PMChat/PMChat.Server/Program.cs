using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PMChat.Models;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.SessionContainer;

namespace PMChat.Server
{
    class Program
    {
        private static List<ClientInfo> _clients = new List<ClientInfo>();
        private static ISessionContainer _sessionContainer = null;
        static async Task Main(string[] args)
        {
            var host = SuperSocketHostBuilder.Create<TextPackageInfo, LinePipelineFilter>(args)
                .ConfigureSuperSocket(options =>
                {
                    options.AddListener(new ListenOptions
                        {
                            Ip = "Any",
                            Port = 4040
                        })
                        .AddListener(new ListenOptions()
                        {
                            Ip = "Any",
                            Port = 8888
                        });
                })
                .UseSession<MySession>()
                .UseSessionHandler(async s =>
                {
                    var data = new TcpPackage()
                    {
                        OpCode = OpCode.Connect,
                        LocalName = "Server",
                        RemoteName = "All",
                        MessageType = MessageType.Text,
                        Message = String.Empty,
                        Clients = _clients
                    };

                    var sessions = _sessionContainer.GetSessions();
                    foreach (var session in sessions)
                    {
                        string da = data.ToString();
                        await session.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(da)));
                    }
                }, async (s, e) =>
                {
                    _clients = _clients.Where(x => x.SessionId != s.SessionID).ToList();

                    var sessions = _sessionContainer.GetSessions().Where(x => x.SessionID != s.SessionID);
                    var data = new TcpPackage()
                    {
                        OpCode = OpCode.Connect,
                        LocalName = "Server",
                        RemoteName = "All",
                        MessageType = MessageType.Text,
                        Message = "Connect Success.",
                        Clients = _clients.Count() == 1 ? null : _clients
                    };
                    foreach (var session in sessions)
                    {
                        string val = data.ToString();
                        await session.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(val)));
                    }
                })
                .UsePackageHandler(async (s, p) =>
                {
                    var package = TcpPackage.JsonToPackage(p.Text);

                    switch (package.OpCode)
                    {
                        case OpCode.Connect:
                            _clients.Add(new ClientInfo()
                            {
                                Username = package.LocalName,
                                SessionId = s.SessionID
                            });
                            var sessions = _sessionContainer.GetSessions();
                            foreach (var session in sessions)
                            {
                                var connectData = new TcpPackage()                                {
                                    OpCode = OpCode.Connect,
                                    MessageType = MessageType.Text,
                                    LocalName = "Server",
                                    RemoteName = _clients.Where(x => s.SessionID == x.SessionId)?.FirstOrDefault() != null 
                                        ? _clients.FirstOrDefault(x => s.SessionID == x.SessionId)?.Username : null,
                                    Message = "Connect Success.",
                                    Clients = _clients.Where(x => x.SessionId != session.SessionID).ToList()
                                };
                                string conn = connectData.ToString();
                                await session.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(conn)));
                            }
                            break;
                        case OpCode.All:
                            TcpPackage allData = null;
                            switch (package.MessageType)
                            {
                                case MessageType.Text:
                                    allData = new TcpPackage()
                                    {
                                        OpCode = OpCode.All,
                                        MessageType = MessageType.Text,
                                        LocalName = package.LocalName,
                                        RemoteName = "All",
                                        Message = package.Message
                                    };
                                    break;
                                case MessageType.Image:
                                    var imgConfig = new UdpConfigPackage()
                                    {
                                        SendEndPoint = $"{((IPEndPoint)s.LocalEndPoint).Address}|{11000}",
                                        ReceiveEndPoint = $"{((IPEndPoint)s.RemoteEndPoint).Address}|{12000}"
                                    };
                                    allData = new TcpPackage()
                                    {
                                        OpCode = OpCode.All,
                                        MessageType = MessageType.Image,
                                        LocalName = "Server",
                                        RemoteName = package.LocalName,
                                        Config = imgConfig
                                    };
                                    break;
                                case MessageType.File:
                                    allData = new TcpPackage()
                                    {
                                        OpCode = OpCode.All,
                                        MessageType = MessageType.File,
                                        LocalName = "Server",
                                        RemoteName = package.LocalName,
                                        Message = s.LocalEndPoint.ToString()
                                    };
                                    break;
                            }

                            var aSessionClients =
                                _sessionContainer.GetSessions().Where(x => x.SessionID != s.SessionID);
                            foreach (var sClient in aSessionClients)
                            {
                                string val = allData.ToString();
                                await sClient.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(val)));
                            }
                            break;
                        case OpCode.Single:
                            TcpPackage singleData = null;
                            switch (package.MessageType)
                            {
                                case MessageType.Text:
                                    singleData = new TcpPackage()
                                    {
                                        OpCode = OpCode.Single,
                                        MessageType = MessageType.Text,
                                        LocalName = package.LocalName,
                                        RemoteName = package.RemoteName,
                                        Message = package.Message
                                    };
                                    break;
                                case MessageType.Image:
                                    var imgConfig = new UdpConfigPackage()
                                    {
                                        SendEndPoint = $"{((IPEndPoint)s.LocalEndPoint).Address}.{11000}",
                                        ReceiveEndPoint = $"{((IPEndPoint)s.RemoteEndPoint).Address}.{12000}"
                                    };
                                    singleData = new TcpPackage()
                                    {
                                        OpCode = OpCode.Single,
                                        MessageType = MessageType.Image,
                                        LocalName = "Server",
                                        RemoteName = package.LocalName,
                                        Message = imgConfig.ToString()
                                    };
                                    break;
                                case MessageType.File:
                                    singleData = new TcpPackage()
                                    {
                                        OpCode = OpCode.Single,
                                        MessageType = MessageType.File,
                                        LocalName = package.LocalName,
                                        RemoteName = package.RemoteName,
                                        Message = s.LocalEndPoint.ToString()
                                    };
                                    break;
                            }
                            
                            var remoteSession = 
                                _clients.Where(y => y.Username == package.RemoteName);
                            foreach (var rSession in remoteSession)
                            {
                                var sSessionClients = _sessionContainer.GetSessions()
                                    .Where(x => x.SessionID.Equals(rSession.SessionId));
                                foreach (var sClient in sSessionClients)
                                {
                                    var sing = singleData.ToString();
                                    await sClient.SendAsync(
                                        new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(sing)));
                                }
                            }
                            break;
                        case OpCode.DisConnect:
                            break;
                        default:
                            throw new ArgumentException(message: "op code error");
                    }
                })
                .ConfigureErrorHandler((s, v) =>
                {
                    Console.WriteLine($"\n[{DateTime.Now}] [TCP] Error信息:" + s.SessionID.ToString() + Environment.NewLine);
                    return default;
                })
                .UseMiddleware<InProcSessionContainerMiddleware>()
                .UseInProcSessionContainer()
                .BuildAsServer();

            _sessionContainer = host.GetSessionContainer();

            await host.StartAsync();

#pragma warning disable 4014
            Thread th = new Thread(() => Send(host));
#pragma warning restore 4014
            th.Start();

            if (Console.ReadKey().KeyChar.Equals('q'))
            {
                th.Abort();
                await host.StopAsync();
            }
        }

        static async Task Send(IServer host)
        {
            while (true)
            {
                //if (sessions.Count != 0)
                //{
                //    foreach (var session in sessions)
                //    {
                //        await session.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("Send Form Server" + "\r\n")));
                //    }
                //}
                
                var currentProcess = Process.GetCurrentProcess();
                Console.WriteLine($"\n[{DateTime.Now}] RAM:{currentProcess.PrivateMemorySize64 / 1024 / 1024}/MB" + Environment.NewLine);
                Thread.Sleep(3000);
            }
        }
    }
}
