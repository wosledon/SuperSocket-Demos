using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server;

namespace Chat.Server
{
    class Program
    {
        private static List<IAppSession> _sessions = new List<IAppSession>();
        private static int _msgCount = 0; // 消息序号
        private static int _tcpCount = 0;
        private static int _udpCount = 0;

        static async Task Main(string[] args)
        {
            var host = SuperSocketHostBuilder.Create<TextPackageInfo, LinePipelineFilter>(args)
                .ConfigureSuperSocket(options =>
                {
                    options.AddListener(new ListenOptions
                        {
                            Ip = "Any",
                            Port = 4041
                        });
                })
                .UseSession<MySession>()
                .UseSessionHandler(onConnected: (s) =>
                {
                    _sessions.Add(s);
                    Console.WriteLine($"\n[{++_msgCount}] [TCP] 客户端上线:" + _sessions.Count + Environment.NewLine);
                    _tcpCount++;
                    return default;
                }, onClosed: (s, e) =>
                {
                    if (_sessions != null)
                    {
                        foreach (var session in _sessions)
                        {
                            if (session.SessionID.Equals(s.SessionID))
                            {
                                _sessions.Remove(session);
                                break;
                            }
                        }
                    }
                    Console.WriteLine($"\n[{++_msgCount}] [TCP] 客户端下线:" + _sessions.Count + Environment.NewLine);
                    _tcpCount--;
                    return default;
                })
                //.UseSessionHandler(onClosed: (s,e) =>
                //{
                //    foreach (var session in sessions)
                //    {
                //        if (session.SessionID.Equals(s.SessionID))
                //        {
                //            sessions.Remove(session);
                //            break;
                //        }
                //    }
                //    return default;
                //})
                .UsePackageHandler(async (s, p) =>
                {
                    Console.WriteLine($"\n[{++_msgCount}] [TCP] 服务器信息:" + p.Text + Environment.NewLine);
                    if (_sessions.Count != 0)
                    {
                        foreach (var session in _sessions)
                        {
                            if (session.SessionID != s.SessionID)
                            {
                                await session.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(p.Text + "\r\n")));
                            }
                        }
                    }
                })
                .ConfigureErrorHandler((s, v) =>
                {
                    Console.WriteLine($"\n[{++_msgCount}] [TCP] Error信息:" + s.SessionID.ToString() + Environment.NewLine);
                    return default;
                })
                .BuildAsServer();

            //var sessionContainer = host.GetSessionContainer();
            //Console.WriteLine(sessionContainer?.GetSessionCount());
            //if (sessionContainer != null)
            //{
            //    var sessions = sessionContainer.GetSessions();
            //    foreach (var appSession in sessions)
            //    {
            //        Console.WriteLine(appSession.ToString());
            //    }
            //}

            var hostUdp = SuperSocketHostBuilder.Create<TextPackageInfo, LinePipelineFilter>(args)
                .ConfigureSuperSocket(options =>
                {
                    options.AddListener(new ListenOptions
                    {
                        Ip = "Any",
                        Port = 4042
                    });
                })
                .UsePackageHandler(async (s, p) =>
                {
                    Console.WriteLine($"\n[{++_msgCount}] [UDP] 服务器信息:" + p.Text + Environment.NewLine);
                })
                .UseSessionHandler(onConnected: (s) =>
                {
                    _sessions.Add(s);
                    Console.WriteLine($"\n[{++_msgCount}] [UDP] 客户端上线:" + _sessions.Count + Environment.NewLine);
                    _udpCount++;
                    return default;
                }, onClosed: (s, e) =>
                {
                    foreach (var session in _sessions)
                    {
                        if (session.SessionID.Equals(s.SessionID))
                        {
                            _sessions.Remove(session);
                            break;
                        }
                    }
                    Console.WriteLine($"\n[{++_msgCount}] [UDP] 客户端下线:" + _sessions.Count + Environment.NewLine);
                    _udpCount--;
                    return default;
                })
                .UseUdp()
                .BuildAsServer();


            await host.StartAsync();
            await hostUdp.StartAsync();

#pragma warning disable 4014
            Thread th = new Thread(() => Send());
#pragma warning restore 4014
            th.Start();

            if (Console.ReadKey().KeyChar.Equals('q'))
            {
                th.Abort();
                await host.StopAsync();
                await hostUdp.StartAsync();
            }

        }

        static async Task Send()
        {
            while (true) 
            {
                Console.WriteLine($"\n[{++_msgCount}] 客户端存活:{_sessions.Count} [TCP]:{_tcpCount}  [UDP]:{_udpCount}" + Environment.NewLine);
                //if (sessions.Count != 0)
                //{
                //    foreach (var session in sessions)
                //    {
                //        await session.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("Send Form Server" + "\r\n")));
                //    }
                //}

                Thread.Sleep(3000);
            }
        }
    }
}
