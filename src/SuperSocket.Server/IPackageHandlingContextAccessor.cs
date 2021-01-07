using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.Server
{

    public interface IPackageHandlingContextAccessor<TPackageInfo> 
    {
        /// <summary>
        /// 包处理上下文
        /// </summary>
        PackageHandlingContext<IAppSession, TPackageInfo> PackageHandlingContext { get; set; }
    }


}
