using System;
using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public class TerminatorTextPipelineFilter : TerminatorPipelineFilter<TextPackageInfo>
    {
        /// <summary>
        /// 终端文本管道筛选器
        /// </summary>
        /// <param name="terminator"></param>
        public TerminatorTextPipelineFilter(ReadOnlyMemory<byte> terminator)
            : base(terminator)
        {

        }
        /// <summary>
        /// 解析包
        /// </summary>
        /// <param name="buffer">数据</param>
        /// <returns>文本信息</returns>
        protected override TextPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            return new TextPackageInfo { Text = buffer.GetString(Encoding.UTF8) };
        }
    }
}
