using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO.Pipelines;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    /// <summary>
    /// 通道设置
    /// </summary>
    public class ChannelOptions
    {
        // 4M by default
        /// <summary>
        /// 包大小默认4M
        /// </summary>
        public int MaxPackageLength { get; set; } = 1024 * 1024 * 4;
        
        // 4k by default
        /// <summary>
        /// 接收的字节码大小默认4K
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 1024 * 4;

        // 4k by default
        /// <summary>
        /// 发送的字节码大小默认4K
        /// </summary>
        public int SendBufferSize { get; set; } = 1024 * 4;

        // trigger the read only when the stream is being consumed
        /// <summary>
        /// 仅在使用Stream时触发
        /// </summary>
        public bool ReadAsDemand { get; set; }
        
        /// <summary>
        /// in milliseconds
        /// 接收超时，以毫秒为单位
        /// </summary>
        /// <value></value>
        public int ReceiveTimeout { get; set; }

        /// <summary>
        /// in milliseconds
        /// 发送超时，以毫秒为单位
        /// </summary>
        /// <value></value>
        public int SendTimeout { get; set; }
        /// <summary>
        /// 日志
        /// </summary>
        public ILogger Logger { get; set; }
        /// <summary>
        /// 管道进
        /// </summary>
        public Pipe In { get; set; }
        /// <summary>
        /// 管道出
        /// </summary>
        public Pipe Out { get; set; }
    }
}
