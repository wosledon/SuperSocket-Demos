using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    public class WebSocketPackage : IWebSocketFrameHeader
    {
        public OpCode OpCode { get; set; }

        internal byte OpCodeByte { get; set; }

        public bool FIN
        {
            get { return ((OpCodeByte & 0x80) == 0x80); }
            set
            {
                if (value)
                    OpCodeByte = (byte)(OpCodeByte | 0x80);
                else
                    OpCodeByte = (byte)(OpCodeByte ^ 0x80);
            }
        }

        public bool RSV1
        {
            get { return ((OpCodeByte & 0x40) == 0x40); }
            set
            {
                if (value)
                    OpCodeByte = (byte)(OpCodeByte | 0x40);
                else
                    OpCodeByte = (byte)(OpCodeByte ^ 0x40);
            }
        }

        public bool RSV2
        {
            get { return ((OpCodeByte & 0x20) == 0x20); }
            set
            {
                if (value)
                    OpCodeByte = (byte)(OpCodeByte | 0x20);
                else
                    OpCodeByte = (byte)(OpCodeByte ^ 0x20);
            }
        }

        public bool RSV3
        {
            get { return ((OpCodeByte & 0x10) == 0x10); }
            set
            {
                if (value)
                    OpCodeByte = (byte)(OpCodeByte | 0x10);
                else
                    OpCodeByte = (byte)(OpCodeByte ^ 0x10);
            }
        }
        /// <summary>
        /// 保存操作码字节
        /// </summary>
        internal void SaveOpCodeByte()
        {
            OpCodeByte = (byte)((OpCodeByte & 0xF0) | (byte)OpCode);
        }

        internal bool HasMask { get; set; }

        internal long PayloadLength { get; set; }

        internal byte[] MaskKey { get; set; }

        public string Message { get; set; }

        public HttpHeader HttpHeader { get; set; }

        public ReadOnlySequence<byte> Data { get; set; }

        internal SequenceSegment Head { get; set; }

        internal SequenceSegment Tail { get; set; }
        /// <summary>
        /// 拼接序列
        /// </summary>
        /// <param name="second"></param>
        internal void ConcatSequence(ref ReadOnlySequence<byte> second)
        {
            if (Head == null)
            {
                (Head, Tail) = second.DestructSequence();
                return;
            }

            if (!second.IsEmpty)
            {
                foreach (var segment in second)
                {
                    Tail = Tail.SetNext(new SequenceSegment(segment));
                }
            }
        }
        /// <summary>
        /// 生成数据
        /// </summary>
        internal void BuildData()
        {
            Data = new ReadOnlySequence<byte>(Head, 0, Tail, Tail.Memory.Length);
        }
    }
}
