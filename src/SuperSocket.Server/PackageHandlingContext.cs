using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Server
{
    public class PackageHandlingContext<TAppSession, TPackageInfo>
    {
        /// <summary>
        /// 初始化包处理上下文
        /// </summary>
        /// <param name="appSession">Session</param>
        /// <param name="packageInfo">包信息</param>
        public PackageHandlingContext(TAppSession appSession, TPackageInfo packageInfo)
        {
            AppSession = appSession;
            PackageInfo = packageInfo;
        }

        public TAppSession AppSession { get; }

        public TPackageInfo PackageInfo { get; }
    }
}
