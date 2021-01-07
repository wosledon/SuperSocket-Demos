using System;
using System.Threading.Tasks;

namespace SuperSocket.Command
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class AsyncCommandFilterAttribute : CommandFilterBaseAttribute
    {
        /// <summary>
        /// Called when [command executing].
        /// 命令执行时
        /// </summary>
        /// <param name="commandContext">命令上下文</param>
        /// <returns>return if the service should continue to process this session.如果服务继续处理此Seesion时返回</returns>
        public abstract ValueTask<bool> OnCommandExecutingAsync(CommandExecutingContext commandContext);

        /// <summary>
        /// Called when [command executed].
        /// 命令执行后
        /// </summary>
        /// <param name="commandContext">The command context.命令上下文</param>
        public abstract ValueTask OnCommandExecutedAsync(CommandExecutingContext commandContext);
    }
}
