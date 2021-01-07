using Microsoft.Extensions.Logging;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface IChannelCreatorFactory
    {
        /// <summary>
        /// 创建通道创造器
        /// </summary>
        /// <typeparam name="TPackageInfo">包信息</typeparam>
        /// <param name="options">监听设置</param>
        /// <param name="channelOptions">通道设置</param>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="pipelineFilterFactory">管道筛选器工厂</param>
        /// <returns></returns>
        IChannelCreator CreateChannelCreator<TPackageInfo>(ListenOptions options, ChannelOptions channelOptions, ILoggerFactory loggerFactory, object pipelineFilterFactory);
    }
}