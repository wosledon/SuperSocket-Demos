using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Server
{
    public class PackageHandlingContextAccessor<TPackageInfo> : IPackageHandlingContextAccessor<TPackageInfo>
    {
        private static AsyncLocal<PackageHandlingContextHolder> AppSessionCurrent { get; set; } = new AsyncLocal<PackageHandlingContextHolder>();

        /// <summary>
        /// 初始化包处理上下文
        /// </summary>
        PackageHandlingContext<IAppSession, TPackageInfo> IPackageHandlingContextAccessor<TPackageInfo>.PackageHandlingContext
        {
            get
            {
                return AppSessionCurrent.Value?.Context;
            }
            set
            {
                var holder = AppSessionCurrent.Value;
                if (holder != null)
                {
                    holder.Context = null;
                }

                if (value != null)
                {
                    AppSessionCurrent.Value = new PackageHandlingContextHolder { Context = value };
                }
            }
        }

        private class PackageHandlingContextHolder
        {
            /// <summary>
            /// 初始化包处理上下文
            /// </summary>
            public PackageHandlingContext<IAppSession, TPackageInfo> Context { get; set; }
        }
    }


}
