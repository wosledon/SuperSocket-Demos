using System;
using System.Text;

namespace Chat.UdpDemo
{
    [Serializable]
    public class UdpPackage
    {
        public int PackageIdentityNum { get; set; }
        public int PackageSerialNum { get; set; }
        public int PackageCount { get; set; }
        public byte[] PackageBuffer { get; set; }
    }
}