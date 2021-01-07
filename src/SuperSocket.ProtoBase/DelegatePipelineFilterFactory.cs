using System;

namespace SuperSocket.ProtoBase
{
    public class DelegatePipelineFilterFactory<TPackageInfo> : PipelineFilterFactoryBase<TPackageInfo>
    {
        private readonly Func<object, IPipelineFilter<TPackageInfo>> _factory;
        /// <summary>
        /// 管道筛选器工厂委托
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="factory"></param>
        public DelegatePipelineFilterFactory(IServiceProvider serviceProvider, Func<object, IPipelineFilter<TPackageInfo>> factory)
            : base(serviceProvider)
        {
            _factory = factory;
        }
        /// <summary>
        /// 创建管道筛选器核心
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected override IPipelineFilter<TPackageInfo> CreateCore(object client)
        {
            return _factory(client);
        }
    }
}