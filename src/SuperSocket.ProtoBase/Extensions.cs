using System;
using System.Buffers;
using System.Text;
using System.Buffers.Text;

namespace SuperSocket.ProtoBase
{

    public static class Extensions
    {
        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="reader">数据</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public static string ReadString(ref this SequenceReader<byte> reader, long length = 0)
        {
            return ReadString(ref reader, Encoding.UTF8, length);
        }
        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="reader">数据</param>
        /// <param name="encoding">字符编码</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public static string ReadString(ref this SequenceReader<byte> reader, Encoding encoding, long length = 0)
        {
            if (length == 0)
                length = reader.Remaining;

            var seq = reader.Sequence.Slice(reader.Consumed, length);
            
            try
            {                
                return seq.GetString(encoding);
            }
            finally
            {
                reader.Advance(length);
            }
        }
        /// <summary>
        /// 读取大端数据（数据必须为256的整数倍）
        /// </summary>
        /// <param name="reader">数据</param>
        /// <param name="value">unshort</param>
        /// <returns>是否读取成功</returns>
        public static bool TryReadBigEndian(ref this SequenceReader<byte> reader, out ushort value)
        {
            value = 0;

            if (reader.Remaining < 2)
                return false;

            if (!reader.TryRead(out byte h))
                return false;

            if (!reader.TryRead(out byte l))
                return false;

            value = (ushort)(h * 256 + l);
            return true;
        }
        /// <summary>
        /// 读取大端数据
        /// </summary>
        /// <param name="reader">数据</param>
        /// <param name="value">uint</param>
        /// <returns></returns>
        public static bool TryReadBigEndian(ref this SequenceReader<byte> reader, out uint value)
        {
            value = 0;

            if (reader.Remaining < 4)
                return false;
            
            var v = 0;

            for (var i = 4; i >= 0; i--)
            {
                if (!reader.TryRead(out byte b))
                    return false;
                
                var unit = 2 ^ i;
                v += unit * b;
            }

            value = (uint)v;
            return true;
        }
        /// <summary>
        /// 读取大端数据
        /// </summary>
        /// <param name="reader">数据</param>
        /// <param name="value">ulong</param>
        /// <returns></returns>
        public static bool TryReadBigEndian(ref this SequenceReader<byte> reader, out ulong value)
        {
            value = 0;

            if (reader.Remaining < 8)
                return false;
            
            var v = 0L;

            for (var i = 8; i >= 0; i--)
            {
                if (!reader.TryRead(out byte b))
                    return false;
                
                var unit = 2 ^ i;
                v += unit * b;
            }

            value = (ulong)v;
            return true;
        }
        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <param name="buffer">数据</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string GetString(this ReadOnlySequence<byte> buffer, Encoding encoding)
        {
            if (buffer.IsSingleSegment)
            {
                return encoding.GetString(buffer.First.Span);
            }

            if (encoding.IsSingleByte)
            {
                return string.Create((int)buffer.Length, buffer, (span, sequence) =>
                {
                    foreach (var segment in sequence)
                    {
                        var count = encoding.GetChars(segment.Span, span);
                        span = span.Slice(count);
                    }
                });
            }

            var sb = new StringBuilder();
            var decoder = encoding.GetDecoder();

            foreach (var piece in buffer)
            {                
                var charBuff = (new char[piece.Length]).AsSpan();
                var len = decoder.GetChars(piece.Span, charBuff, false);                
                sb.Append(new string(len == charBuff.Length ? charBuff : charBuff.Slice(0, len)));
            }

            return sb.ToString();
        }
        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="writer">写入器</param>
        /// <param name="text">文本信息</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static int Write(this IBufferWriter<byte> writer, ReadOnlySpan<char> text, Encoding encoding)
        {
            var encoder = encoding.GetEncoder();
            var completed = false;
            var totalBytes = 0;

            var minSpanSizeHint = encoding.GetMaxByteCount(1);

            while (!completed)
            {
                var span = writer.GetSpan(minSpanSizeHint);

                encoder.Convert(text, span, false, out int charsUsed, out int bytesUsed, out completed);
                
                if (charsUsed > 0)
                    text = text.Slice(charsUsed);

                totalBytes += bytesUsed;
                writer.Advance(bytesUsed);
            }

            return totalBytes;
        }
    }
}