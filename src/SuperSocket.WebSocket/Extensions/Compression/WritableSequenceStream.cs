using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.Extensions.Compression
{
    class WritableSequenceStream : Stream
    {
        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length { get => throw new NotSupportedException(); }

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        private SequenceSegment _head;
        
        private SequenceSegment _tail;

        private static readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
        /// <summary>
        /// 清除缓存区
        /// </summary>
        public override void Flush()
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 读
        /// </summary>
        /// <param name="buffer">缓冲区</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">读取个数</param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 将该流的当前位置设置为给定值
        /// </summary>
        /// <param name="offset">移动距离(字节为单位)</param>
        /// <param name="origin">开始位置</param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 设置长度
        /// </summary>
        /// <param name="value">值</param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 写
        /// </summary>
        /// <param name="buffer">缓冲区</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">写入个数</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            var data = _arrayPool.Rent(count);

            Array.Copy(buffer, offset, data, 0, count);

            var segment = new SequenceSegment(data, count);

            if (_head == null)
                _tail = _head = segment;
            else
                _tail.SetNext(segment);
        }

        public ReadOnlySequence<byte> GetUnderlyingSequence()
        {
            return new ReadOnlySequence<byte>(_head, 0, _tail, _tail.Memory.Length);
        }
    }
}
