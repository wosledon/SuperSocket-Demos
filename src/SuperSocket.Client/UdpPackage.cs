using System;
using System.IO;
using System.Linq;

namespace SuperSocket.Client
{
    public class UdpPackage: IUdpPackage
    {
        /// <summary>
        /// 文件传输标识
        /// </summary>
        public byte FileIdentity { get; set; }
        /// <summary>
        /// Udp操作码
        /// </summary>
        public UdpOpCode OpCode { get; set; } = UdpOpCode.Message;
        /// <summary>
        /// 文件切块时的编号
        /// </summary>
        public ushort BlockSerial { get; set; }
        /// <summary>
        /// 块切片后的编号
        /// </summary>
        public ushort SliceSerial { get; set; }
        /// <summary>
        /// 块切片的数量
        /// </summary>
        public ushort SliceCount { get; set; }
        /// <summary>
        /// 携带的数据
        /// </summary>
        public byte[] Buffer { get; set; }
        public byte[] PackageToBytes()
        {
            byte[] buffer = new byte[8 + Buffer.Length];
            BitConverter.GetBytes(FileIdentity).CopyTo(buffer, 0);
            BitConverter.GetBytes((byte)OpCode).CopyTo(buffer, 1);
            BitConverter.GetBytes(BlockSerial).CopyTo(buffer, 2);
            BitConverter.GetBytes(SliceSerial).CopyTo(buffer, 4);
            BitConverter.GetBytes(SliceCount).CopyTo(buffer, 6);
            Buffer.CopyTo(buffer, 8);
            return buffer;
        }

        public Object BytesToUdpPackage(byte[] buffer)
        {
            FileIdentity = buffer[0];
            OpCode = (UdpOpCode)buffer[1];
            BlockSerial = BitConverter.ToUInt16(buffer, 2);
            SliceSerial = BitConverter.ToUInt16(buffer, 4);
            SliceCount = BitConverter.ToUInt16(buffer, 6);
            Buffer = buffer.Skip(8).ToArray();

            return this;
        }
    }
}