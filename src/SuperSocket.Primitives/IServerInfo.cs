using System;

namespace SuperSocket
{
    public interface IServerInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 数据上下文
        /// </summary>
        object DataContext { get; set; }
        /// <summary>
        /// Session数量
        /// </summary>
        int SessionCount { get; }
        /// <summary>
        /// ServiceProvider
        /// </summary>
        IServiceProvider ServiceProvider { get; }
        /// <summary>
        /// 服务器状态
        /// </summary>
        ServerState State { get; }
    }
}