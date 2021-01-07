using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public class SequenceSegment : ReadOnlySequenceSegment<byte>, IDisposable
    {
        private bool disposedValue;

        private byte[] _pooledBuffer;

        private bool _pooled = false;
        /// <summary>
        /// 初始化序列段
        /// </summary>
        /// <param name="pooledBuffer">汇集Buffer</param>
        /// <param name="length">长度</param>
        /// <param name="pooled">是否汇集</param>
        public SequenceSegment(byte[] pooledBuffer, int length, bool pooled)
        {
            _pooledBuffer = pooledBuffer;
            _pooled = pooled;
            this.Memory = new ArraySegment<byte>(pooledBuffer, 0, length);
        }
        /// <summary>
        /// 初始化序列段
        /// </summary>
        /// <param name="pooledBuffer">汇集buffer</param>
        /// <param name="length">长度</param>
        public SequenceSegment(byte[] pooledBuffer, int length)
            : this(pooledBuffer, length, true)
        {

        }
        /// <summary>
        /// 初始化序列段
        /// </summary>
        /// <param name="memory">内存</param>
        public SequenceSegment(ReadOnlyMemory<byte> memory)
        {
            this.Memory = memory;
        }
        /// <summary>
        /// 初始化序列段
        /// </summary>
        /// <param name="segment">分段</param>
        /// <returns></returns>
        public SequenceSegment SetNext(SequenceSegment segment)
        {
            segment.RunningIndex = RunningIndex + Memory.Length;
            Next = segment;
            return segment;
        }
        /// <summary>
        /// 初始化序列段
        /// </summary>
        /// <param name="memory">内存</param>
        /// <returns></returns>
        public static SequenceSegment CopyFrom(ReadOnlyMemory<byte> memory)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(memory.Length);
            memory.Span.CopyTo(new Span<byte>(buffer));
            return new SequenceSegment(buffer, memory.Length);
        }
        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_pooled && _pooledBuffer != null)
                        ArrayPool<byte>.Shared.Return(_pooledBuffer);
                }

                disposedValue = true;
            }
        }
        /// <summary>
        /// GC回收
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
