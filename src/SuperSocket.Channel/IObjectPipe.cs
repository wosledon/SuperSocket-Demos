using System;
using System.Threading.Tasks;

namespace SuperSocket.Channel
{
    interface IObjectPipe<T>
    {
        /// <summary>
        /// Write an object into the pipe
        /// 向管道内写入对象
        /// </summary>
        /// <param name="target">the object tp be added into the pipe. 要写入的对象</param>
        /// <returns>pipe's length, how many objects left in the pipe. 管道的长度</returns>
        int Write(T target);
        /// <summary>
        /// 异步读取对象
        /// </summary>
        /// <returns></returns>
        ValueTask<T> ReadAsync();
    }

    interface ISupplyController
    {
        ValueTask SupplyRequired();

        void SupplyEnd();
    }
}
