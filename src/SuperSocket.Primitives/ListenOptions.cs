using System.Security.Authentication;

namespace SuperSocket
{
    public class ListenOptions
    {
        /// <summary>
        /// IP地址
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 返回日志
        /// </summary>
        public int BackLog { get; set; }
        /// <summary>
        /// 是否有延迟
        /// </summary>
        public bool NoDelay { get; set; }
        /// <summary>
        /// 安全
        /// </summary>
        public SslProtocols Security { get; set; }
        /// <summary>
        /// 证书选项配置
        /// </summary>
        public CertificateOptions CertificateOptions { get; set; }
                
        /// <summary>
        /// 转为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{nameof(Ip)}={Ip}, {nameof(Port)}={Port}, {nameof(Security)}={Security}, {nameof(Path)}={Path}, {nameof(BackLog)}={BackLog}, {nameof(NoDelay)}={NoDelay}";
        }
    }
}