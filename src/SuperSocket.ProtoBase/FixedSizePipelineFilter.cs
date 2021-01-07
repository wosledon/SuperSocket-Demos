using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public class FixedSizePipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo>
        where TPackageInfo : class
    {
        private int _size;
        /// <summary>
        /// 初始化固定大小管道筛选器
        /// </summary>
        /// <param name="size"></param>
        protected FixedSizePipelineFilter(int size)
        {
            _size = size;
        }
        /// <summary>
        /// 筛选器
        /// </summary>
        /// <param name="reader">数据</param>
        /// <returns>数据包</returns>
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            if (reader.Length < _size)
                return null;

            var pack = reader.Sequence.Slice(0, _size);

            try
            {
                return DecodePackage(ref pack);
            }
            finally
            {
                reader.Advance(_size);
            }
        }
    }
}