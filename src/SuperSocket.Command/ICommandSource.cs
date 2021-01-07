using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperSocket.Command
{
    public interface ICommandSource
    {
        /// <summary>
        /// 获取命令的类型
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria);
    }
}
