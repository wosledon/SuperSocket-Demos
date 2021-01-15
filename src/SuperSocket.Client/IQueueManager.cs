using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public interface IQueueManager
    {
        Task ClearAsync();
        Task<bool> ContainsAsync(byte[] buffer);
        Task<byte[]> DequeueAsync();
        Task EnqueueAsync(byte[] buffer);
        Task<byte[][]> ToArrayAsync();
        int Length();

        //Task MultiThreadingDequeue(IUdpPackageManager<IUdpPackage> packageManager, IUdpClientManager udpClientManager);
        //Task MultiThreadingEnqueue(IUdpPackageManager<IUdpPackage> packageManager, IUdpClientManager udpClientManager);
    }

    public interface IQueueManager<TPackage>
    {
        Task ClearAsync();
        Task<bool> ContainsAsync(TPackage package);
        Task<TPackage> DequeueAsync();
        Task EnqueueAsync(TPackage package);
        Task<TPackage[]> ToArrayAsync();
        Task<int> Length();

        //Task MultiThreadingDequeue(IUdpPackageManager<IUdpPackage> packageManager, IUdpClientManager udpClientManager);
        //Task MultiThreadingEnqueue(IUdpPackageManager<IUdpPackage> packageManager, IUdpClientManager udpClientManager);
    }
}