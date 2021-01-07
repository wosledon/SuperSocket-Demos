using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public interface IConnector
    {
/// <summary>
/// 异步连接
/// </summary>
/// <param name="remoteEndPoint">远程网络标识</param>
/// <param name="state">连接状态</param>
/// <param name="cancellationToken">异步操作标识Token</param>
/// <returns></returns>
ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state = null, CancellationToken cancellationToken = default);
/// <summary>
/// 下一个连接
/// </summary>
IConnector NextConnector { get; }
    }
}