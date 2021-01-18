using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public class TerminatorPipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo>
        where TPackageInfo : class
    {
        private readonly ReadOnlyMemory<byte> _terminator;
        /// <summary>
        /// 初始化终端管道筛选器
        /// </summary>
        /// <param name="terminator">终端信息</param>
        public TerminatorPipelineFilter(ReadOnlyMemory<byte> terminator)
        {
            _terminator = terminator;
        }
        /// <summary>
        /// 筛选器
        /// </summary>
        /// <param name="reader">数据</param>
        /// <returns></returns>
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            var terminator =  _terminator;
            var terminatorSpan = terminator.Span;

            if (!reader.TryReadTo(out ReadOnlySequence<byte> pack, terminatorSpan, advancePastDelimiter: false))
                return null;

            try
            {
                return DecodePackage(ref pack);
            }
            finally
            {
                reader.Advance(terminator.Length);
            }
        }
    }
}