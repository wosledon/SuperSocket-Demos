using System;
using System.Buffers;
using JT808.Protocol;
using JT808.Protocol.Extensions;
using SuperSocket.ProtoBase;
using JT808.Protocol.Internal;

namespace JT808.Socket.Server
{
    public class MyPipelineFilter : BeginEndMarkPipelineFilter<JT808Package>
    {
        private static readonly byte[] BeginMark = new byte[] { 0x7E };
        private static readonly byte[] EndMark = new byte[] { 0x7E };

        public JT808Serializer Jt808Serializer;
        public MyPipelineFilter()
            : base(BeginMark, EndMark)
        {
            Jt808Serializer = new JT808Serializer();
        }

        protected override JT808Package DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            //byte[] bytes = "7E 01 00 40 54 01 00 00 00 00 01 77 70 64 12 11 01 00 00 09 00 0A 36 33 36 33 36 36 34 36 38 31 31 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 41 42 43 59 42 41 55 55 51 30 58 52 4F 50 44 42 59 4D 4C 43 4B 55 53 42 4E 59 41 34 57 4C 41 50 55 02 B2 E2 41 38 34 34 36 31 20 7E".ToHexBytes();
            //byte[] buff = buffer.ToArray();
            //var msg = buff.ToString();

            var buff = buffer.ToArray();
            var data = new byte[buff.Length + 2];
            new byte[] { 0x7E }.CopyTo(data, 0);
            buff.CopyTo(data, 1);
            new byte[] { 0x7E }.CopyTo(data, buff.Length+1);


            var jT808Package = Jt808Serializer.Deserialize(data);
            return jT808Package;
        }
    }
}