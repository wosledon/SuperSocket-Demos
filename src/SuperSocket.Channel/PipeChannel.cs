using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;


[assembly: InternalsVisibleTo("Test")] 
namespace SuperSocket.Channel
{
    public abstract partial class PipeChannel<TPackageInfo> : ChannelBase<TPackageInfo>, IChannel<TPackageInfo>, IChannel, IPipeChannel
    {
        private IPipelineFilter<TPackageInfo> _pipelineFilter;

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        protected Pipe Out { get; }

        Pipe IPipeChannel.Out
        {
            get { return Out; }
        }

        protected Pipe In { get; }

        Pipe IPipeChannel.In
        {
            get { return In; }
        }

        IPipelineFilter IPipeChannel.PipelineFilter
        {
            get { return _pipelineFilter; }
        }

        private IObjectPipe<TPackageInfo> _packagePipe;

        protected ILogger Logger { get; }

        protected ChannelOptions Options { get; }

        private Task _readsTask;

        private Task _sendsTask;

        private bool _isDetaching = false;
        /// <summary>
        /// 设置管道
        /// </summary>
        /// <param name="pipelineFilter">管道筛选器</param>
        /// <param name="options">通道选项</param>
        protected PipeChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
        {
            _pipelineFilter = pipelineFilter;

            if (!options.ReadAsDemand)
                _packagePipe = new DefaultObjectPipe<TPackageInfo>();
            else
                _packagePipe = new DefaultObjectPipeWithSupplyControl<TPackageInfo>();

            Options = options;
            Logger = options.Logger;
            Out = options.Out ?? new Pipe();
            In = options.In ?? new Pipe();
        }
        /// <summary>
        /// 开始管道
        /// </summary>
        public override void Start()
        {
            _readsTask = ProcessReads();
            _sendsTask = ProcessSends();
            WaitHandleClosing();
        }
        /// <summary>
        /// 等待处理关闭中
        /// </summary>
        private async void WaitHandleClosing()
        {
            await HandleClosing();
        }
        /// <summary>
        /// 异步运行
        /// </summary>
        /// <returns>数据包</returns>
        public async override IAsyncEnumerable<TPackageInfo> RunAsync()
        { 
            if (_readsTask == null || _sendsTask == null)
                throw new Exception("The channel has not been started yet.");

            while (true)
            {
                var package = await _packagePipe.ReadAsync().ConfigureAwait(false);

                if (package == null)
                {
                    await HandleClosing();
                    yield break;
                }

                yield return package;
            }
        }
        /// <summary>
        /// 处理关闭
        /// </summary>
        /// <returns></returns>
        private async ValueTask HandleClosing()
        {
            try
            {
                await Task.WhenAll(_readsTask, _sendsTask);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                OnError("Unhandled exception in the method PipeChannel.Run.", e);
            }
            finally
            {
                if (!_isDetaching && !IsClosed)
                    OnClosed();
            }
        }

        protected abstract void Close();
        /// <summary>
        /// 异步关闭
        /// </summary>
        /// <param name="closeReason">关闭原因</param>
        /// <returns></returns>
        public override async ValueTask CloseAsync(CloseReason closeReason)
        {
            CloseReason = closeReason;
            Close();            
            _cts.Cancel();
            await HandleClosing();
        }
        /// <summary>
        /// 充满管道
        /// </summary>
        /// <param name="writer">管道写入器</param>
        /// <returns></returns>
        protected virtual async Task FillPipeAsync(PipeWriter writer)
        {
            var options = Options;
            var cts = _cts;

            var supplyController = _packagePipe as ISupplyController;

            if (supplyController != null)
            {
                cts.Token.Register(() =>
                {
                    supplyController.SupplyEnd();
                });
            }

            while (!cts.IsCancellationRequested)
            {
                try
                {                    
                    if (supplyController != null)
                    {
                        await supplyController.SupplyRequired();

                        if (cts.IsCancellationRequested)
                            break;
                    }                        

                    var bufferSize = options.ReceiveBufferSize;
                    var maxPackageLength = options.MaxPackageLength;

                    if (bufferSize <= 0)
                        bufferSize = 1024 * 4; //4k

                    var memory = writer.GetMemory(bufferSize);

                    var bytesRead = await FillPipeWithDataAsync(memory, cts.Token);         

                    if (bytesRead == 0)
                    {
                        if (!CloseReason.HasValue)
                            CloseReason = Channel.CloseReason.RemoteClosing;
                        
                        break;
                    }

                    LastActiveTime = DateTimeOffset.Now;
                    
                    // Tell the PipeWriter how much was read
                    writer.Advance(bytesRead);
                }
                catch (Exception e)
                {
                    if (!IsIgnorableException(e))
                    {
                        OnError("Exception happened in ReceiveAsync", e);

                        if (!CloseReason.HasValue)
                        {
                            CloseReason = cts.IsCancellationRequested
                                ? Channel.CloseReason.LocalClosing : Channel.CloseReason.SocketError; 
                        }
                    }
                    else if (!CloseReason.HasValue)
                    {
                        CloseReason = Channel.CloseReason.RemoteClosing;
                    }
                    
                    break;
                }

                // Make the data available to the PipeReader
                var result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Signal to the reader that we're done writing
            writer.Complete();
            Out.Writer.Complete();// TODO: should complete the output right now?
        }
        /// <summary>
        /// 是否忽略异常
        /// </summary>
        /// <param name="e">异常信息</param>
        /// <returns>true:忽略；false:不忽略；</returns>
        protected virtual bool IsIgnorableException(Exception e)
        {
            if (e is ObjectDisposedException || e is NullReferenceException || e is OperationCanceledException)
                return true;

            if (e.InnerException != null)
                return IsIgnorableException(e.InnerException);

            return false;
        }
        /// <summary>
        /// 数据充满管道
        /// </summary>
        /// <param name="memory">内存</param>
        /// <param name="cancellationToken">异步操作标识Token</param>
        /// <returns></returns>
        protected abstract ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken);
        /// <summary>
        /// 过程读取
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ProcessReads()
        {
            var pipe = In;

            Task writing = FillPipeAsync(pipe.Writer);
            Task reading = ReadPipeAsync(pipe.Reader);

            await Task.WhenAll(reading, writing);
        }
        /// <summary>
        /// 过程发送
        /// </summary>
        /// <returns></returns>
        protected async Task ProcessSends()
        {
            var output = Out.Reader;
            var cts = _cts;

            while (!cts.IsCancellationRequested)
            {
                var result = await output.ReadAsync(cts.Token);

                if (result.IsCanceled)
                    break;

                var completed = result.IsCompleted;

                var buffer = result.Buffer;
                var end = buffer.End;
                
                if (!buffer.IsEmpty)
                {
                    try
                    {
                        await SendOverIOAsync(buffer, cts.Token);
                        LastActiveTime = DateTimeOffset.Now;
                    }
                    catch (Exception e)
                    {
                        output.Complete(e);
                        cts.Cancel(false);
                        
                        if (!IsIgnorableException(e))
                            OnError("Exception happened in SendAsync", e);
                        
                        return;
                    }
                }

                output.AdvanceTo(end);

                if (completed)
                {
                    break;
                }
            }

            output.Complete();
        }

        /// <summary>
        /// 检查通道是否打开
        /// </summary>
        private void CheckChannelOpen()
        {
            if (this.IsClosed)
            {
                throw new Exception("Channel is closed now, send is not allowed.");
            }
        }
        /// <summary>
        /// 异步发送IO
        /// </summary>
        /// <param name="buffer">数据</param>
        /// <param name="cancellationToken">异步操作标识Token</param>
        /// <returns></returns>
        protected abstract ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken);

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据</param>
        /// <returns></returns>
        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
        {
            try
            {
                await _sendLock.WaitAsync();
                var writer = Out.Writer;
                WriteBuffer(writer, buffer);
                await writer.FlushAsync();
            }
            finally
            {
                _sendLock.Release();
            }            
        }
        /// <summary>
        /// 写入buffer
        /// </summary>
        /// <param name="writer">通道写入器</param>
        /// <param name="buffer">数据</param>
        private void WriteBuffer(PipeWriter writer, ReadOnlyMemory<byte> buffer)
        {
            CheckChannelOpen();
            writer.Write(buffer.Span);
        }
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <typeparam name="TPackage">数据包类型</typeparam>
        /// <param name="packageEncoder">包编码</param>
        /// <param name="package">数据包</param>
        /// <returns></returns>
        public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            try
            {
                await _sendLock.WaitAsync();
                var writer = Out.Writer;
                WritePackageWithEncoder<TPackage>(writer, packageEncoder, package);
                await writer.FlushAsync();
            }
            finally
            {
                _sendLock.Release();
            }
        }
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="write">管道写入器</param>
        /// <returns></returns>
        public override async ValueTask SendAsync(Action<PipeWriter> write)
        {
            try
            {
                await _sendLock.WaitAsync();
                var writer = Out.Writer;
                write(writer);
                await writer.FlushAsync();
            }
            finally
            {
                _sendLock.Release();
            }
        }
        /// <summary>
        /// 根据编码写入数据包
        /// </summary>
        /// <typeparam name="TPackage">数据包类型</typeparam>
        /// <param name="writer">管道写入器</param>
        /// <param name="packageEncoder">数据包编码格式</param>
        /// <param name="package">数据包</param>
        private void WritePackageWithEncoder<TPackage>(PipeWriter writer, IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            CheckChannelOpen();
            packageEncoder.Encode(writer, package);
        }
        /// <summary>
        /// 从内存中读取数组段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memory">内存</param>
        /// <returns></returns>
        protected internal ArraySegment<T> GetArrayByMemory<T>(ReadOnlyMemory<T> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }

            return result;
        }
        /// <summary>
        /// 异步读取管道
        /// </summary>
        /// <param name="reader">管道读取器</param>
        /// <returns></returns>
        protected async Task ReadPipeAsync(PipeReader reader)
        {
            var cts = _cts;

            while (!cts.IsCancellationRequested)
            {
                ReadResult result;

                try
                {
                    result = await reader.ReadAsync(cts.Token);
                }
                catch (Exception e)
                {
                    if (!IsIgnorableException(e))
                        OnError("Failed to read from the pipe", e);
                    
                    break;
                }

                var buffer = result.Buffer;

                SequencePosition consumed = buffer.Start;
                SequencePosition examined = buffer.End;

                if (result.IsCanceled)
                {
                    break;
                }

                var completed = result.IsCompleted;

                try
                {
                    if (buffer.Length > 0)
                    {
                        if (!ReaderBuffer(ref buffer, out consumed, out examined))
                        {
                            completed = true;
                            break;
                        }                        
                    }

                    if (completed)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    OnError("Protocol error", e);
                    // close the connection if get a protocol error
                    Close();
                    break;
                }
                finally
                {
                    reader.AdvanceTo(consumed, examined);
                }
            }

            reader.Complete();
            WriteEOFPackage();
        }
        /// <summary>
        /// 写EOF包（结束包，EOF: End of File）
        /// </summary>
        protected void WriteEOFPackage()
        {
            _packagePipe.Write(default);
        }
        /// <summary>
        /// 读取Buffer
        /// </summary>
        /// <param name="buffer">数据</param>
        /// <param name="consumed">消耗</param>
        /// <param name="examined">检查</param>
        /// <returns>true:成功；false:失败；</returns>
        private bool ReaderBuffer(ref ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            var bytesConsumedTotal = 0L;

            var maxPackageLength = Options.MaxPackageLength;

            var seqReader = new SequenceReader<byte>(buffer);

            while (true)
            {
                var currentPipelineFilter = _pipelineFilter;
                var filterSwitched = false;

                var packageInfo = currentPipelineFilter.Filter(ref seqReader);

                var nextFilter = currentPipelineFilter.NextFilter;

                if (nextFilter != null)
                {
                    nextFilter.Context = currentPipelineFilter.Context; // pass through the context
                    _pipelineFilter = nextFilter;
                    filterSwitched = true;
                }

                var bytesConsumed = seqReader.Consumed;
                bytesConsumedTotal += bytesConsumed;

                var len = bytesConsumed;

                // nothing has been consumed, need more data
                if (len == 0)
                    len = seqReader.Length;

                if (maxPackageLength > 0 && len > maxPackageLength)
                {
                    OnError($"Package cannot be larger than {maxPackageLength}.");
                    // close the the connection directly
                    Close();
                    return false;
                }            
                
                if (packageInfo == null)
                {
                    // the current pipeline filter needs more data to process
                    if (!filterSwitched)
                    {
                        // set consumed position and then continue to receive...
                        consumed = buffer.GetPosition(bytesConsumedTotal);
                        return true;
                    }
                    
                    // we should reset the previous pipeline filter after switch
                    currentPipelineFilter.Reset();
                }
                else
                {
                    // reset the pipeline filter after we parse one full package
                    currentPipelineFilter.Reset();
                    _packagePipe.Write(packageInfo);
                }

                if (seqReader.End) // no more data
                {
                    examined = consumed = buffer.End;
                    return true;
                }
                
                if (bytesConsumed > 0)
                    seqReader = new SequenceReader<byte>(seqReader.Sequence.Slice(bytesConsumed));
            }
        }
        /// <summary>
        /// 异步分离
        /// </summary>
        /// <returns></returns>
        public override async ValueTask DetachAsync()
        {
            _isDetaching = true;
            _cts.Cancel();
            await HandleClosing();
            _isDetaching = false;
        }
        /// <summary>
        /// 触发错误，写入日志
        /// </summary>
        /// <param name="message">描述</param>
        /// <param name="e">错误信息</param>
        protected void OnError(string message, Exception e = null)
        {
            if (e != null)
                Logger?.LogError(e, message);
            else
                Logger?.LogError(message);
        }
    }
}
