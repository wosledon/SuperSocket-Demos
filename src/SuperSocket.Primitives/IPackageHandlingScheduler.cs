using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface IPackageHandlingScheduler<TPackageInfo>
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="packageHandler">包处理</param>
        /// <param name="errorHandler">错误处理</param>
        void Initialize(IPackageHandler<TPackageInfo> packageHandler, Func<IAppSession, PackageHandlingException<TPackageInfo>, ValueTask<bool>> errorHandler);
        /// <summary>
        /// 处理包
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="package">Package</param>
        /// <returns></returns>
        ValueTask HandlePackage(IAppSession session, TPackageInfo package);
    }
}