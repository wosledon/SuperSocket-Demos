using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public interface IPipelineFilter
    {
        /// <summary>
        /// 重置
        /// </summary>
        void Reset();
        /// <summary>
        /// 上下文
        /// </summary>
        object Context { get; set; }        
    }

    public interface IPipelineFilter<TPackageInfo> : IPipelineFilter
    {
        /// <summary>
        /// 解码器
        /// </summary>
        IPackageDecoder<TPackageInfo> Decoder { get; set; }
        /// <summary>
        /// 筛选器
        /// </summary>
        /// <param name="reader">序列阅读器</param>
        /// <returns></returns>
        TPackageInfo Filter(ref SequenceReader<byte> reader);
        /// <summary>
        /// 下一个筛选
        /// </summary>
        IPipelineFilter<TPackageInfo> NextFilter { get; }
        
    }
}