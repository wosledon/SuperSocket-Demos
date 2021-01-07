using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public interface IPipeChannel
    {
        /// <summary>
        /// 管道进
        /// </summary>
        Pipe In { get; }
        /// <summary>
        /// 管道出
        /// </summary>
        Pipe Out { get; }
        /// <summary>
        /// 管道筛选器
        /// </summary>
        IPipelineFilter PipelineFilter { get; }
    }
}
