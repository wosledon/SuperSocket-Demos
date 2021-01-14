using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public class UdpClientManager: UdpClient, IUdpClientManager
    {
        public UdpClientManager()
        {
            
        }

        public UdpClient UdpClient { get; set; }
        public async Task<UdpClient> CreateUdpSendClientAsync(int port)
        {
            
            return await Task.Run(() =>
            {
                UdpClient = new UdpClient(port);
                return UdpClient;
            });
        }

        public async Task<UdpClient> CreateUdpSendClientAsync(IPEndPoint localEndPoint)
        {
            return await Task.Run(() => new UdpClient(localEndPoint));
        }

        public async Task UdpSendClientConnectAsync(IPEndPoint remoteEndPoint)
        {
            await Task.Run(() => Client.Connect(remoteEndPoint));
        }

        public async Task UdpSendClientSendAsync(byte[] buffer)
        {
            await Task.Run(() => UdpClient.SendAsync(buffer, buffer.Length));
        }

        public async Task<UdpClient> CreateUdpReceiveClientAsync(int port)
        {
            return await Task.Run(() =>
            {
                UdpClient = new UdpClient(port);
                return UdpClient;
            });
        }

        public async Task<byte[]> ReceiveClientReceiveAsync()
        {
            var result = await UdpClient.ReceiveAsync();
            return result.Buffer;
        }

        public async Task UdpClientCloseAsync()
        {
            await Task.Run(()=>UdpClient.Close());
        }

        public async Task<IUdpClientManager> AsUdpClientManager()
        {
            return await Task.Run(() => this);
        }

        public async Task<UdpClient> AsUdpClient()
        {
            return await Task.Run(() => UdpClient);
        }
    }
}