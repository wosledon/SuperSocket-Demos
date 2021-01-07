using System;

namespace SuperSocket.Command
{
    public struct CommandExecutingContext
    {
        /// <summary>
        /// Gets the session.
        /// 获取Session
        /// </summary>
        public IAppSession Session { get; set; }

        /// <summary>
        /// Gets the request info.
        /// 获取请求消息
        /// </summary>
        public object Package { get; set; }

        /// <summary>
        /// Gets the current command.
        /// 获取当前命令
        /// </summary>
        public ICommand CurrentCommand { get; set; }

        /// <summary>
        /// Gets the exception.
        /// 获取异常
        /// </summary>
        /// <value>
        /// The exception.异常信息
        /// </value>
        public Exception Exception { get; set; }
    }
}
