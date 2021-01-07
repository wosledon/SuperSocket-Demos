using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Channel
{
    public static class Extensions
    {
        /// <summary>
        /// 获取数据包Stream（异步）
        /// </summary>
        /// <typeparam name="TPackageInfo">包信息</typeparam>
        /// <param name="channel">管道</param>
        /// <returns>数据包流</returns>
        public static IAsyncEnumerator<TPackageInfo> GetPackageStream<TPackageInfo>(this IChannel<TPackageInfo> channel)
            where TPackageInfo : class
        {
            return channel.RunAsync().GetAsyncEnumerator();
        }
        /// <summary>
        /// 异步接收
        /// </summary>
        /// <typeparam name="TPackageInfo">包信息</typeparam>
        /// <param name="packageStream">数据包Stream</param>
        /// <returns>数据包信息</returns>
        public static async ValueTask<TPackageInfo> ReceiveAsync<TPackageInfo>(this IAsyncEnumerator<TPackageInfo> packageStream)
        {
            if (await packageStream.MoveNextAsync())
                return packageStream.Current;

            return default(TPackageInfo);
        }
    }
}
