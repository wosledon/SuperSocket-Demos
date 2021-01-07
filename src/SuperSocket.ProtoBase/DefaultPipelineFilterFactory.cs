using System;

namespace SuperSocket.ProtoBase
{
    public class DefaultPipelineFilterFactory<TPackageInfo, TPipelineFilter> : PipelineFilterFactoryBase<TPackageInfo>
        where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
    {
        /// <summary>
        /// 默认管道筛选器工厂
        /// </summary>
        /// <param name="serviceProvider"></param>
        public DefaultPipelineFilterFactory(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {

        }
        /// <summary>
        /// 创建管道筛选器核心
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected override IPipelineFilter<TPackageInfo> CreateCore(object client)
        {
            return new TPipelineFilter();
        }
    }
}