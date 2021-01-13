namespace UdpBlockTransferClient
{
    public enum TransferState: ushort
    {
        Start = 0,
        Body = 1,
        End = 2
    }
}