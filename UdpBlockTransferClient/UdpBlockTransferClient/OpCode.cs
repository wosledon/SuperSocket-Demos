namespace UdpBlockTransferClient
{
    public enum OpCode: byte
    {
        Notice = 0,
        Message = 1,
        Start = 2,
        End = 3
    }
}