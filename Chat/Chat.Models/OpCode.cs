namespace Chat.Models
{
    public enum OpCode: byte
    {
        Connect = 1,
        DisConnect = 2,
        Subscribe = 3,
        Single = 4,
        All = 5
    }
}
