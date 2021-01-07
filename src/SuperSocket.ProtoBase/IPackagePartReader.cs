using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public interface IPackagePartReader<TPackageInfo>
    {
        /// <summary>
        /// 阅读过程
        /// </summary>
        /// <param name="package">包</param>
        /// <param name="filterContext">筛选器上下文</param>
        /// <param name="reader">阅读器</param>
        /// <param name="nextPartReader">下一个阅读器</param>
        /// <param name="needMoreData">需要更多数据</param>
        /// <returns></returns>
        bool Process(TPackageInfo package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<TPackageInfo> nextPartReader, out bool needMoreData);
    }
}