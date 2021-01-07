using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public class LinePipelineFilter : TerminatorPipelineFilter<TextPackageInfo>
    {
        /// <summary>
        /// 编码格式
        /// </summary>
        protected Encoding Encoding { get; private set; }

        public LinePipelineFilter()
            : this(Encoding.UTF8)
        {

        }
        /// <summary>
        /// 行管道筛选器
        /// </summary>
        /// <param name="encoding">编码格式</param>
        public LinePipelineFilter(Encoding encoding)
            : base(new[] { (byte)'\r', (byte)'\n' })
        {
            Encoding = encoding;
        }
        /// <summary>
        /// 对包进行解码
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        protected override TextPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            return new TextPackageInfo { Text = buffer.GetString(Encoding) };
        }
    }
}
