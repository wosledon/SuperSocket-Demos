using System;
using System.Buffers;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace JT808.Socket.Client
{
    class Program
    {
        static int count = 0;
        private static IEasyClient<TextPackageInfo> client;
        static async Task Main(string[] args)
        {
            client = new EasyClient<TextPackageInfo>(new MyPackageFilter()).AsClient();

            if (!await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 4040)))
            {
                Console.WriteLine("Failed to connect the target server.");
                return;
            }

            //client.PackageHandler += async (s, p) =>
            //{
            //    Console.WriteLine(p.Text);
            //    await Task.CompletedTask;
            //};
            //client.StartReceive();

            //var hdn = Task.Run(Receive);
            //hdn.Start();



            await client.SendAsync(Encoding.UTF8.GetBytes("Test" + "\r\n"));
            //await client.SendAsync(Encoding.UTF8.GetBytes("Test" + "\r\n"));
            //await Task.Delay(5000);
            //var p = await client.ReceiveAsync();
            //Console.WriteLine(p.Text);
            await Task.Delay(5000);

            while (true)
            {
                var p = await client.ReceiveAsync();
                if (p == null)
                {
                    break;
                }
                Console.WriteLine("接收到的数据:" + p.Text);
            }

            //while (true)
            //{
            //    var p = await client.ReceiveAsync();
            //    Console.WriteLine("接收信息:" + p.Text?.ToString());
            //}
        }

    }

    public enum OpCode : byte
    {
        Connect = 1,
        Subscribe = 2,
        Publish = 3
    }

    public class MyPackage
    {
        public OpCode Code { get; set; }

        public short Sequence { get; set; }

        public string Body { get; set; }
    }

    public class MyPackageFilter : FixedHeaderPipelineFilter<TextPackageInfo>
    {
        /// <summary>
        /// Header size is 5
        /// 1: OpCode
        /// 2-3: body length
        /// 4-5: sequence
        /// </summary>
        public MyPackageFilter()
            : base(5)
        {

        }

        protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);

            reader.Advance(1); // skip the first byte for OpCode
            reader.TryReadBigEndian(out short len);
            reader.Advance(2); // skip the two bytes for Sequence

            return len;
        }

        protected override TextPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            var package = new TextPackageInfo();

            var reader = new SequenceReader<byte>(buffer);

            //reader.TryRead(out byte opCodeByte);
            //package.Code = (OpCode)opCodeByte;

            // skip the two bytes for length, we don't need length any more
            // because we already get the full data of the package in the buffer
            reader.Advance(2);

            //reader.TryReadBigEndian(out short sequence);
            //package.Sequence = sequence;
            // get the rest of the data in the reader and then read it as utf8 string
            package.Text = reader.ReadString(Encoding.UTF8);

            return package;
        }
    }
}
