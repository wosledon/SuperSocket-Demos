using System;

namespace SuperSocket.ProtoBase
{
    public class CommandLinePipelineFilter : TerminatorPipelineFilter<StringPackageInfo>
    {
        /// <summary>
        /// 命令行管道筛选器
        /// </summary>
        public CommandLinePipelineFilter()
            : base(new[] { (byte)'\r', (byte)'\n' })
        {

        }
    }
}
