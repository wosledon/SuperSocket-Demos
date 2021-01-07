using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public class DelegatePackageHandler<TReceivePackageInfo> : IPackageHandler<TReceivePackageInfo>
    {

        Func<IAppSession, TReceivePackageInfo, ValueTask> _func;
        /// <summary>
        /// 包处理委托
        /// </summary>
        /// <param name="func"></param>
        public DelegatePackageHandler(Func<IAppSession, TReceivePackageInfo, ValueTask> func)
        {
            _func = func;
        }
        /// <summary>
        /// 异步处理
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="package">接受包信息</param>
        /// <returns></returns>
        public async ValueTask Handle(IAppSession session, TReceivePackageInfo package)
        {
            await _func(session, package);
        }
    }
}