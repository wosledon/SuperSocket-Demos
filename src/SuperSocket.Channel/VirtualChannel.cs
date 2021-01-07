using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public abstract class VirtualChannel<TPackageInfo> : PipeChannel<TPackageInfo>, IVirtualChannel
    {
/// <summary>
/// 初始化虚拟通道
/// </summary>
/// <param name="pipelineFilter">管道筛选器</param>
/// <param name="options">通道选项</param>
public VirtualChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
    : base(pipelineFilter, options)
{
 
}
/// <summary>
/// 数据充满管道
/// </summary>
/// <param name="writer">管道写入器</param>
/// <returns>Task完成标识</returns>
protected override Task FillPipeAsync(PipeWriter writer)
{
    return Task.CompletedTask;
}
/// <summary>
/// 向管道内写入数据
/// </summary>
/// <param name="memory">内存</param>
/// <param name="cancellationToken">异步操作标识Token</param>
/// <returns></returns>
public async ValueTask<FlushResult> WritePipeDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
{
    return await In.Writer.WriteAsync(memory, cancellationToken);
}
    }
}