using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;

namespace UdpBlockTransferClient
{
    /// <summary>
    /// 数据格式: 2+1+2+2+1+N 小于1500
    /// +------+------+------+------+------+------+
    /// |2 Byte|1 Byte|2 Byte|2 Byte|2 Byte|N Byte|
    /// +------+------+------+------+------+------+
    /// |文件标识|操作码 | 块号 |切片序号|切片数量| 数据 |
    /// +------+------+------+------+------+------+
    /// </summary>
    public class UdpPackage
    {
        /// <summary>
        /// 文件标识, 用于区分发送的文件
        /// </summary>
        public ushort PackageIdentity { get; set; }

        public byte PackageOpCode { get; set; } = (byte)OpCode.Message;

        /// <summary>
        /// 块号, 用于文件分块接收标识, 用于组装文件
        /// 0: 不分块
        /// 1~N: 块号
        /// </summary>
        public ushort PackageBlockSerial { get; set; } = 0;

        /// <summary>
        /// 块切片序号, 用于组装块
        /// </summary>
        public ushort PackageSliceSerial { get; set; } = 0;

        /// <summary>
        /// 块切片数量, 丢失重传
        /// </summary>
        public ushort PackageSliceCount { get; set; } = 0;

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] PackageBody { get; set; } = null;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[9 + PackageBody.Length];
            BitConverter.GetBytes(PackageIdentity).CopyTo(buffer, 0);
            BitConverter.GetBytes(PackageOpCode).CopyTo(buffer,2);
            BitConverter.GetBytes(PackageBlockSerial).CopyTo(buffer, 3);
            BitConverter.GetBytes(PackageSliceSerial).CopyTo(buffer, 5);
            BitConverter.GetBytes(PackageSliceCount).CopyTo(buffer, 7);
            PackageBody.CopyTo(buffer, 9);

            return buffer;
        }

        public UdpPackage BytesToUdpPackage(byte[] buffer)
        {
            PackageIdentity = BitConverter.ToUInt16(buffer, 0);
            PackageOpCode = buffer[2];
            PackageBlockSerial = BitConverter.ToUInt16(buffer, 3);
            PackageSliceSerial = BitConverter.ToUInt16(buffer, 5);
            PackageSliceCount =BitConverter.ToUInt16(buffer, 7);
            PackageBody = buffer.Skip(9).ToArray();

            return this;
        }
    }
}