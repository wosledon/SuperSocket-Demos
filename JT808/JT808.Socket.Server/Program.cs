using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JT808.Protocol;
using JT808.Protocol.Enums;
using JT808.Protocol.Extensions;
using JT808.Protocol.MessageBody;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.SessionContainer;

namespace JT808.Socket.Server
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var host = SuperSocketHostBuilder.Create<JT808Package, MyPipelineFilter>(args)
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
                .UseSessionHandler( s =>
                {
                    s["Identify"] = "0x001";
                    return default;
                })
                .UsePackageHandler(async (s, p) =>
                {
                    #region 解包/应答/转发
                    Console.WriteLine(p.ToString());
                    JT808Package jT808Package = JT808MsgId.位置信息汇报.Create("123456789012",
                        new JT808_0x0200
                        {
                            AlarmFlag = 1,
                            Altitude = 40,
                            GPSTime = DateTime.Parse("2018-10-15 10:10:10"),
                            Lat = 12222222,
                            Lng = 132444444,
                            Speed = 60,
                            Direction = 0,
                            StatusFlag = 2,
                            JT808LocationAttachData = new Dictionary<byte, JT808_0x0200_BodyBase>
                            {
                                { JT808Constants.JT808_0x0200_0x01,new JT808_0x0200_0x01{Mileage = 100}},
                                { JT808Constants.JT808_0x0200_0x02,new JT808_0x0200_0x02{Oil = 125}}
                            }
                        });
                    jT808Package.Header.ManualMsgNum = 1;
                    byte[] data = new JT808Serializer().Serialize(jT808Package);
                    await s.SendAsync(new ReadOnlyMemory<byte>(data));
                    Console.WriteLine(s["Identify"].ToString());
                    #endregion
                })
                .ConfigureErrorHandler((s, v) =>
                {
                    Console.WriteLine($"\n[{DateTime.Now}] [TCP] Error信息:" + s.SessionID.ToString() + Environment.NewLine);
                    return default;
                })
                .UseMiddleware<InProcSessionContainerMiddleware>()
                .UseInProcSessionContainer()
                .BuildAsServer();

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
                var currentProcess = Process.GetCurrentProcess();
                Console.WriteLine($"\n[{DateTime.Now}] RAM:{currentProcess.PrivateMemorySize64 / 1024 / 1024}/MB" + Environment.NewLine);
                Thread.Sleep(5000);
            }
        }
    }
}
