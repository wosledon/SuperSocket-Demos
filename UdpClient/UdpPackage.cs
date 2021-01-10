using System.Collections;
using System.ComponentModel;

namespace UdpClient
{
    /// <summary>
    /// 数据位设计 2+2+2+2+N
    /// +------+------+------+------+------+
    /// |2 Byte|2 Byte|2 Byte|2 Byte|N Byte|
    /// +------+------+------+------+------+
    /// |标识位 |包序号 |文件类型|包数量 |数据  |
    /// +------+------+------+------+------+
    /// </summary>
    public class UdpPackage
    {
        public ushort PackageIdentityNum { get; set; }
        public ushort PackageSerialNum { get; set; }
        public UdpFileModel PackageFileMode { get; set; }
        public ushort PackageCount { get; set; }
        public byte[] PackageBuffer { get; set; }
    }
}