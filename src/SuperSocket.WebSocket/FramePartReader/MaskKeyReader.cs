using System;
using System.Buffers;
using System.Linq;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.FramePartReader
{
    class MaskKeyReader : PackagePartReader
    {
        /// <summary>
        /// 掩码阅读过程
        /// </summary>
        /// <param name="package">WebSocket包</param>
        /// <param name="filterContext">筛选器上下文</param>
        /// <param name="reader">序列阅读器</param>
        /// <param name="nextPartReader">下一分件阅读器</param>
        /// <param name="needMoreData">需要更多数据</param>
        /// <returns></returns>
        public override bool Process(WebSocketPackage package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<WebSocketPackage> nextPartReader, out bool needMoreData)
        {
            int required = 4;

            if (reader.Remaining < required)
            {
                nextPartReader = null;
                needMoreData = true;
                return false;
            }

            needMoreData = false;

            package.MaskKey = reader.Sequence.Slice(reader.Consumed, 4).ToArray();
            reader.Advance(4);

            if (TryInitIfEmptyMessage(package))
            {
                nextPartReader = null;
                return true;
            }

            nextPartReader = PayloadDataReader;
            return false;
        }
    }
}
