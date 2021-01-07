using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public abstract class FixedHeaderPipelineFilter<TPackageInfo> : FixedSizePipelineFilter<TPackageInfo>
        where TPackageInfo : class
    {
        private bool _foundHeader;
        private readonly int _headerSize;
        private int _totalSize;
        /// <summary>
        /// 初始化固定头部管道筛选器
        /// </summary>
        /// <param name="headerSize"></param>
        protected FixedHeaderPipelineFilter(int headerSize)
            : base(headerSize)
        {
            _headerSize = headerSize;
        }
        /// <summary>
        /// 筛选器
        /// </summary>
        /// <param name="reader">数据</param>
        /// <returns>数据包</returns>
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            if (!_foundHeader)
            {
                if (reader.Length < _headerSize)
                    return null;                
                
                var header = reader.Sequence.Slice(0, _headerSize);
                var bodyLength = GetBodyLengthFromHeader(ref header);
                
                if (bodyLength < 0)
                    throw new ProtocolException("Failed to get body length from the package header.");
                
                if (bodyLength == 0)
                {
                    try
                    {
                        return DecodePackage(ref header);
                    }
                    finally
                    {
                        reader.Advance(_headerSize);
                    }                    
                }
                
                _foundHeader = true;
                _totalSize = _headerSize + bodyLength;
            }

            var totalSize = _totalSize;

            if (reader.Length < totalSize)
                return null;

            var pack = reader.Sequence.Slice(0, totalSize);

            try
            {
                return DecodePackage(ref pack);
            }
            finally
            {
                reader.Advance(totalSize);
            } 
        }
        /// <summary>
        /// 从消息头获取消息体长度
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        protected abstract int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer);
        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            
            _foundHeader = false;
            _totalSize = 0;
        }
    }
}