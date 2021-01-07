using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.FramePartReader
{
    class ExtendedLengthReader : PackagePartReader
    {
        /// <summary>
        /// 读取过程
        /// </summary>
        /// <param name="package">WebSocket包</param>
        /// <param name="filterContext">筛选器上下文</param>
        /// <param name="reader">序列阅读器</param>
        /// <param name="nextPartReader">下一分件阅读器</param>
        /// <param name="needMoreData">需要更多数据</param>
        /// <returns></returns>
        public override bool Process(WebSocketPackage package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<WebSocketPackage> nextPartReader, out bool needMoreData)
        {
            int required;

            if (package.PayloadLength == 126)
                required = 2;
            else
                required = 8;

            if (reader.Remaining < required)
            {
                nextPartReader = null;
                needMoreData = true;
                return false;
            }

            needMoreData = false;

            if (required == 2)
            {
                reader.TryReadBigEndian(out ushort len);
                package.PayloadLength = len;
            }
            else // required == 8 (long)
            {
                reader.TryReadBigEndian(out long len);
                package.PayloadLength = len;
            }

            if (package.HasMask)
                nextPartReader = MaskKeyReader;
            else
                nextPartReader = PayloadDataReader;

            return false;
        }
    }
}
