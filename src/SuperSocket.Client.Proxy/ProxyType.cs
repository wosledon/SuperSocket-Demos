using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client.Proxy
{
    /// <summary>
    /// 代理类型枚举
    /// </summary>
    public enum ProxyType
    {
        Http,
        Socks4,
        Socks4a,
        Socks5
    }
}