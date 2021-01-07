using System;

namespace SuperSocket.Command
{
    public interface ICommandFilter
    {
        int Order { get; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class CommandFilterBaseAttribute : Attribute, ICommandFilter
    {
        /// <summary>
        /// Gets or sets the execution order.
        /// 获取或设置执行清单
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }
    }


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class CommandFilterAttribute : CommandFilterBaseAttribute
    {
        /// <summary>
        /// Called when [command executing].
        /// 命令执行时
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        public abstract bool OnCommandExecuting(CommandExecutingContext commandContext);

        /// <summary>
        /// Called when [command executed].
        /// 命令执行后
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        public abstract void OnCommandExecuted(CommandExecutingContext commandContext);
    }
}
