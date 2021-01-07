using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public delegate void NewClientAcceptHandler(IChannelCreator listener, IChannel channel);

    public interface IChannelCreator
    {
        /// <summary>
        /// 监听设置
        /// </summary>
        ListenOptions Options { get; }
        /// <summary>
        /// 开始
        /// </summary>
        /// <returns></returns>
        bool Start();
        /// <summary>
        /// 客户端应答处理
        /// </summary>
        event NewClientAcceptHandler NewClientAccepted;
        /// <summary>
        /// 创建通道
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task<IChannel> CreateChannel(object connection);
        /// <summary>
        /// 异步停止
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
        /// <summary>
        /// 通道运行状态
        /// </summary>
        bool IsRunning { get; }
    }
}