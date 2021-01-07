using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    public class SerialPackageHandlingScheduler<TPackageInfo> : PackageHandlingSchedulerBase<TPackageInfo>
    {
        /// <summary>
        /// 处理包
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="package">信息包</param>
        /// <returns></returns>
        public override async ValueTask HandlePackage(IAppSession session, TPackageInfo package)
        {
            await HandlePackageInternal(session, package);
        }
    }
}