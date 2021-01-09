namespace Chat.Client
{
    public class UdpPackage
    {
        public byte[] Bytes { get; set; }
        public int PacketNum { get; set; }
        public int PacketAtNum { get; set; }
        public int PacketCount { get; set; }
    }
}