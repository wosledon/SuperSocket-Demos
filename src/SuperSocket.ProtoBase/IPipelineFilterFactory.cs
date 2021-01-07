namespace SuperSocket.ProtoBase
{
    public interface IPipelineFilterFactory<TPackageInfo>
    {
        /// <summary>
        /// 创建管道筛选器工厂
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        IPipelineFilter<TPackageInfo> Create(object client);
    }
}