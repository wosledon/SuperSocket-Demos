namespace SuperSocket.Client
{
    public interface IUdpPackage
    {
        byte[] PackageToBytes();
        object BytesToUdpPackage(byte[] buffer);
    }
}