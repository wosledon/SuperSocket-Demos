using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SuperSocket
{
    public interface ILoggerAccessor
    {
        /// <summary>
        /// »’÷æ
        /// </summary>
        ILogger Logger { get; }
    }
}