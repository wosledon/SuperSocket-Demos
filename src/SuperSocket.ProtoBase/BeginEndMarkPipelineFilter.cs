using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public abstract class BeginEndMarkPipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo>
        where TPackageInfo : class
    {
        private readonly ReadOnlyMemory<byte> _beginMark;

        private readonly ReadOnlyMemory<byte> _endMark;

        private bool _foundBeginMark;
        /// <summary>
        /// 初始化开始结束标记管道筛选器
        /// </summary>
        /// <param name="beginMark">开始标记</param>
        /// <param name="endMark">结束标记</param>
        protected BeginEndMarkPipelineFilter(ReadOnlyMemory<byte> beginMark, ReadOnlyMemory<byte> endMark)
        {
            _beginMark = beginMark;
            _endMark = endMark;
        }
        /// <summary>
        /// 筛选器
        /// </summary>
        /// <param name="reader">字节序列</param>
        /// <returns>包信息</returns>
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            if (!_foundBeginMark)
            {
                var beginMark = _beginMark.Span;

                if (!reader.IsNext(beginMark, advancePast: true))
                {
                    throw new ProtocolException("Invalid beginning part of the package.");
                }

                _foundBeginMark = true;
            }

            var endMark =  _endMark.Span;

            if (!reader.TryReadTo(out ReadOnlySequence<byte> pack, endMark, advancePastDelimiter: false))
            {
                return null;
            }

            reader.Advance(endMark.Length);
            return DecodePackage(ref pack);
        }
        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            base.Reset();            
            _foundBeginMark = false;
        }
    }
}