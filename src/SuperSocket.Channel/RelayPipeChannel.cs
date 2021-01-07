using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public class RelayPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
        where TPackageInfo : class
    {
/// <summary>
/// 重新建立管道的选项设置
/// </summary>
/// <param name="options">选项</param>
/// <param name="pipeIn">管道入口</param>
/// <param name="pipeOut">管道出口</param>
/// <returns>管道选项设置</returns>
static ChannelOptions RebuildOptionsWithPipes(ChannelOptions options, Pipe pipeIn, Pipe pipeOut)
{
    options.In = pipeIn;
    options.Out = pipeOut;
    return options;
}
/// <summary>
/// 重新建立管道
/// </summary>
/// <param name="pipelineFilter">命令行</param>
/// <param name="options">选项设置</param>
/// <param name="pipeIn">管道入口</param>
/// <param name="pipeOut">管道出口</param>
public RelayPipeChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, Pipe pipeIn, Pipe pipeOut)
    : base(pipelineFilter, RebuildOptionsWithPipes(options, pipeIn, pipeOut))
{

}
/// <summary>
/// 关闭管道进出
/// </summary>
protected override void Close()
{
    In.Writer.Complete();
    Out.Writer.Complete();
}
/// <summary>
/// 将所有数据写入管道
/// </summary>
/// <param name="buffer">只读字符序列</param>
/// <param name="cancellationToken">异步操作标识Token</param>
/// <returns>写入的长度</returns>
protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
{
    var writer = Out.Writer;
    var total = 0;

    foreach (var data in buffer)
    {
        var result = await writer.WriteAsync(data, cancellationToken);

        if (result.IsCompleted)
            total += data.Length;
        else if (result.IsCanceled)
            break;
    }

    return total;
}
/// <summary>
/// 数据充满管道
/// </summary>
/// <param name="memory">内存</param>
/// <param name="cancellationToken">异步操作标识Token</param>
/// <returns></returns>
protected override ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
{
    throw new NotSupportedException();
}
    }
}