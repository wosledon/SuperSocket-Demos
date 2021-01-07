using System;

namespace SuperSocket.ProtoBase
{
    public abstract class PipelineFilterFactoryBase<TPackageInfo> : IPipelineFilterFactory<TPackageInfo>
    {
        /// <summary>
        /// 包解码器
        /// </summary>
        protected IPackageDecoder<TPackageInfo> PackageDecoder { get; private set; }
        /// <summary>
        /// 初始化管道筛选器工厂基类
        /// </summary>
        /// <param name="serviceProvider">Service Provider</param>
        public PipelineFilterFactoryBase(IServiceProvider serviceProvider)
        {
            PackageDecoder = serviceProvider.GetService(typeof(IPackageDecoder<TPackageInfo>)) as IPackageDecoder<TPackageInfo>;
        }
        /// <summary>
        /// 创建核心
        /// </summary>
        /// <param name="client">客户端</param>
        /// <returns></returns>
        protected abstract IPipelineFilter<TPackageInfo> CreateCore(object client);
        /// <summary>
        /// 创建管道筛选器
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public virtual IPipelineFilter<TPackageInfo> Create(object client)
        {
            var filter = CreateCore(client);
            filter.Decoder = PackageDecoder;
            return filter;
        }
    }
}