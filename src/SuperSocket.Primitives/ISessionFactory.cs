using System;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface ISessionFactory
    {
        /// <summary>
        /// 创建一个Session实例
        /// </summary>
        /// <returns></returns>
        IAppSession Create();
        /// <summary>
        /// Session的类型
        /// </summary>
        Type SessionType { get; }
    }
}