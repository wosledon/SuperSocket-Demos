using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface IPackageHandler<TReceivePackageInfo>
    {
        /// <summary>
        /// ´¦Àí
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="package">°ü</param>
        /// <returns></returns>
        ValueTask Handle(IAppSession session, TReceivePackageInfo package);
    }
}