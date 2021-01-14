using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public class QueueManager : IQueueManager
    {
        private Queue<byte[]> Queue { get; set; }

        public QueueManager()
        {
            Queue = new Queue<byte[]>();
        }

        public async Task ClearAsync()
        {
            await Task.Run(() =>
            {
                Queue.Clear();
            });
        }

        public async Task<bool> ContainsAsync(byte[] buffer)
        {
            return await Task.Run(function: () => Queue.Contains(buffer));
        }

        public async Task<byte[]> DequeueAsync()
        {
            return await Task.Run(() => Queue.Dequeue());
        }

        public async Task EnqueueAsync(byte[] buffer)
        {
            await Task.Run(() => Queue.Enqueue(buffer));
        }

        public async Task<byte[][]> ToArrayAsync()
        {
            return await Task.Run(() => Queue.ToArray());
        }

        public async Task<int> Length()
        {
            return await Task.Run(() => Queue.Count);
        }

        //public Task MultiThreadingDequeue(IUdpPackageManager<IUdpPackage> packageManager, IUdpClientManager udpClientManager)
        //{
        //    Object locker = new object();
        //    ThreadPool.QueueUserWorkItem(obj =>
        //    {
        //        while (true)
        //        {
        //            lock (locker)
        //            {
        //                var package = DequeueAsync().Result;
        //                udpClientManager.UdpSendClientSendAsync(package.PackageToBytes());
        //            }
        //        }
        //    });
        //}

        //public Task MultiThreadingEnqueue(Func<IUdpPackageManager<IUdpPackage>, IUdpClientManager> receiveFunc)
        //{
        //    throw new NotImplementedException();
        //}
    }
}