using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public class PackageHandlingException<TPackageInfo> : Exception
    {
        public TPackageInfo Package { get; private set; }
        /// <summary>
        /// 包处理异常
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="package">包</param>
        /// <param name="e">异常</param>
        public PackageHandlingException(string message, TPackageInfo package, Exception e)
            : base(message, e)
        {
            Package = package;
        }
    }
}