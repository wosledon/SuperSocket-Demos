using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public class SecurityOptions : SslClientAuthenticationOptions
    {
        /// <summary>
        /// 用于保存安全认证的凭据
        /// </summary>
        public NetworkCredential Credential { get; set; }
    }
}