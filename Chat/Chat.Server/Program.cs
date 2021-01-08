using Chat.Models;
using Newtonsoft.Json;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.SessionContainer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chat.Server
{
    class Program
    {
        //private static List<IAppSession> _sessions = new List<IAppSession>();
        //private static int _msgCount = 0; // 消息序号
        private static IEnumerable<ClientInfo> _clients = new List<ClientInfo>();
        private static int _tcpCount = 0;
        private static int _udpCount = 0;

        private static ISessionContainer _sessionContainer;

        static async Task Main(string[] args)
        {
            var host = SuperSocketHostBuilder.Create<TextPackageInfo, LinePipelineFilter>(args)
                .ConfigureSuperSocket(options =>
                {
                    options.AddListener(new ListenOptions
                        {
                            Ip = "Any",
                            Port = 4041
                        })
                        .AddListener(new ListenOptions()
                        {
                            Ip = "Any",
                            Port = 8888
                        });
                })
                .UseSession<MySession>()
                .UseSessionHandler(onConnected:async (s) =>
                {
                    Console.WriteLine($"\n[{DateTime.Now}] [TCP] 客户端上线:" + ++_tcpCount + Environment.NewLine);

                    var data = new MessagePackage<TextMessageModel>()
                    {
                        OpCode = OpCode.Connect,
                        MessageType = MessageType.TextMessage,
                        Message = new TextMessageModel()
                        {
                            LocalName = "Server",
                            RemoteName = "All"
                        },
                        Clients = _clients
                    };
                    var sessions = _sessionContainer.GetSessions().Where(x => x.SessionID != s.SessionID);

                    foreach (var session in sessions)
                    {
                        string da = data.ToString();
                        await session.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(da)));
                    }
                }, onClosed: async (s, e) =>
                {
                    _clients = _clients.Where(x => x.SessionId != s.SessionID);

                    var sessions = _sessionContainer.GetSessions().Where(x => x.SessionID != s.SessionID);
                    var data = new MessagePackage<TextMessageModel>()
                    {
                        OpCode = OpCode.Connect,
                        MessageType = MessageType.TextMessage,
                        Message = new TextMessageModel()
                        {
                            LocalName = "Server",
                            TextMessage = "Connect Success."
                        },
                        Clients = _clients.Count() == 1?null:_clients
                    };
                    foreach (var session in sessions)
                    {
                        string val = data.ToString();
                        await session.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(val)));
                    }

                    Console.WriteLine($"\n[{DateTime.Now}] [TCP] 客户端下线:" + --_tcpCount + Environment.NewLine);
                })
                .UsePackageHandler(async (s, p) =>
                {
                    Console.WriteLine($"\n[{DateTime.Now}] [TCP] 服务器信息:" + p.Text + Environment.NewLine);

                    // Connect
                    var package = JsonConvert.DeserializeObject<MessagePackage<TextMessageModel>>(p.Text);
                    
                    switch (package.OpCode)
                    {
                        case OpCode.Connect:
                            _clients = _clients.Concat(new[]
                            {
                                new ClientInfo()
                                {
                                    Username = package.Message.LocalName,
                                    SessionId = s.SessionID
                                }
                            });
                            var sessions = _sessionContainer.GetSessions();
                            foreach (var session in sessions)
                            {
                                var connectData = new MessagePackage<TextMessageModel>()
                                {
                                    OpCode = OpCode.Connect,
                                    MessageType = MessageType.TextMessage,
                                    Message = new TextMessageModel()
                                    {
                                        LocalName = "Server",
                                        RemoteName = _clients.Where(x=>s.SessionID == x.SessionId)?.FirstOrDefault().Username,
                                        TextMessage = "Connect Success."
                                    },
                                    Clients = _clients.Where(x => x.SessionId != session.SessionID)
                                };
                                string conn = connectData.ToString();
                                await session.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(conn)));
                            }
                            break;
                        case OpCode.All:
                            var allData = new MessagePackage<TextMessageModel>()
                            {
                                OpCode = OpCode.All,
                                MessageType = MessageType.TextMessage,
                                Message = new TextMessageModel()
                                {
                                    LocalName = package.Message.LocalName,
                                    RemoteName = "All",
                                    TextMessage = package.Message.TextMessage
                                }
                            };
                            var asessionClients = _sessionContainer.GetSessions().Where(x=>x.SessionID!=s.SessionID);
                            foreach (var sClient in asessionClients)
                            {
                                string val = allData.ToString();
                                await sClient.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(val)));
                            }
                            break;
                        case OpCode.Single:
                            var singleData = new MessagePackage<TextMessageModel>()
                            {
                                OpCode = OpCode.Single,
                                MessageType = MessageType.TextMessage,
                                Message = new TextMessageModel()
                                {
                                    LocalName = package.Message.LocalName,
                                    RemoteName = package.Message.RemoteName,
                                    TextMessage = package.Message.TextMessage
                                }
                            };
                            var remoteSession = _clients.Where(y => y.Username == package.Message.RemoteName);
                            foreach (var rSession in remoteSession)
                            {
                                var ssessionClients = _sessionContainer.GetSessions()
                                    .Where(x => x.SessionID.Equals(rSession.SessionId));
                                foreach (var sClient in ssessionClients)
                                {
                                    var sing = singleData.ToString();
                                    await sClient.SendAsync(
                                        new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(sing)));
                                }
                            }
                            break;
                        case OpCode.Subscribe:
                            default:
                            throw new ArgumentException(message:"op code error");
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


            var hostUdp = SuperSocketHostBuilder.Create<TextPackageInfo, LinePipelineFilter>(args)
                .ConfigureSuperSocket(options =>
                {
                    options.AddListener(new ListenOptions
                    {
                        Ip = "Any",
                        Port = 4042
                    });
                })
                .UsePackageHandler( (s, p) =>
                {
                    Console.WriteLine($"\n[{DateTime.Now}] [UDP] 服务器信息:" + p.Text + Environment.NewLine);
                    return default;
                })
                .UseSessionHandler(onConnected: (s) =>
                {
                    Console.WriteLine($"\n[{DateTime.Now}] [UDP] 客户端上线:" + ++_udpCount + Environment.NewLine);

                    return default;
                }, onClosed: (s, e) =>
                {
                    Console.WriteLine($"\n[{DateTime.Now}] [UDP] 客户端下线:" + --_udpCount + Environment.NewLine);

                    return default;
                })
                .UseUdp()
                .BuildAsServer();


            await host.StartAsync();
            await hostUdp.StartAsync();

#pragma warning disable 4014
            Thread th = new Thread(() => Send(host));
#pragma warning restore 4014
            th.Start();

            if (Console.ReadKey().KeyChar.Equals('q'))
            {
                th.Abort();
                await host.StopAsync();
                await hostUdp.StartAsync();
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
                var data = JsonConvert.SerializeObject(new TextMessageModel
                {
                    LocalName = "Server",
                    TextMessage = "Test Message"
                });
                //var container = host.GetSessionContainer();
                var sessions = _sessionContainer.GetSessions();
                if(sessions.Count() > 0)
                {
                    foreach (var session in sessions)
                    {
                        await session.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(data + "\r\n")));
                    }
                }

                Console.WriteLine($"\n[{DateTime.Now}] 客户端存活:{/*_sessions.Count*/sessions.Count()} [TCP]:{_tcpCount}  [UDP]:{_udpCount}" + Environment.NewLine);
                Console.WriteLine($"\n[{DateTime.Now}] " + JsonConvert.SerializeObject(_clients) + Environment.NewLine);
                var currentProcess = Process.GetCurrentProcess();
                Console.WriteLine($"\n[{DateTime.Now}] RAM:{currentProcess.PrivateMemorySize64 / 1024 / 1024}/MB" + Environment.NewLine);
                Thread.Sleep(3000);
            }
        }
    }
}
