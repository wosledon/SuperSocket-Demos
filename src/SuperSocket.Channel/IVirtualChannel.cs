using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public interface IVirtualChannel : IChannel
    {
/// <summary>
/// 异步写入管道数据
/// </summary>
/// <param name="memory">内存</param>
/// <param name="cancellationToken">异步操作标识Token</param>
/// <returns></returns>
ValueTask<FlushResult> WritePipeDataAsync(Memory<byte> memory, CancellationToken cancellationToken);
    }
}
