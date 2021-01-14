using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public interface IUdpClientManager
    {
        Task<UdpClient> CreateUdpSendClientAsync(int port);
        Task<UdpClient> CreateUdpSendClientAsync(IPEndPoint localEndPoint);
        Task UdpSendClientConnectAsync(IPEndPoint remoteEndPoint);
        Task UdpSendClientSendAsync(byte[] buffer);



        Task<UdpClient> CreateUdpReceiveClientAsync(int port);
        Task<byte[]> ReceiveClientReceiveAsync();
        Task UdpClientCloseAsync();

        Task<IUdpClientManager> AsUdpClientManager();
        Task<UdpClient> AsUdpClient();
    }
}