using System;
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IServer : IServerInfo, IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// “Ï≤Ω∆Ù∂Ø
        /// </summary>
        /// <returns></returns>
        Task<bool> StartAsync();
        /// <summary>
        /// “Ï≤ΩÕ£÷π
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}