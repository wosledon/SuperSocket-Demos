using System;

namespace SuperSocket.ProtoBase
{
    public class ProtocolException : Exception
    {
        /// <summary>
        /// 协议异常
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="exception">异常信息</param>
        public ProtocolException(string message, Exception exception)
            : base(message, exception)
        {

        }
        /// <summary>
        /// 协议异常
        /// </summary>
        /// <param name="message">异常信息</param>
        public ProtocolException(string message)
            : base(message)
        {

        }
    }
}