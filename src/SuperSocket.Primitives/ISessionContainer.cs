using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface ISessionContainer
    {
        /// <summary>
        /// 通过ID获取Session
        /// </summary>
        /// <param name="sessionID">SessionID</param>
        /// <returns></returns>
        IAppSession GetSessionByID(string sessionID);
        /// <summary>
        /// 获取Session数量
        /// </summary>
        /// <returns></returns>
        int GetSessionCount();
        /// <summary>
        /// 获取Session
        /// </summary>
        /// <param name="criteria">标准</param>
        /// <returns></returns>
        IEnumerable<IAppSession> GetSessions(Predicate<IAppSession> criteria = null);
        /// <summary>
        /// 获取Session
        /// </summary>
        /// <typeparam name="TAppSession">Session类型</typeparam>
        /// <param name="criteria">标准</param>
        /// <returns></returns>
        IEnumerable<TAppSession> GetSessions<TAppSession>(Predicate<TAppSession> criteria = null)
            where TAppSession : IAppSession;
    }

    public interface IAsyncSessionContainer
    {
        /// <summary>
        /// 异步通过ID获取Session
        /// </summary>
        /// <param name="sessionID">Session的ID</param>
        /// <returns>Session对象</returns>
        ValueTask<IAppSession> GetSessionByIDAsync(string sessionID);
        /// <summary>
        /// 异步获取Session个数
        /// </summary>
        /// <returns>Session个数</returns>
        ValueTask<int> GetSessionCountAsync();
        /// <summary>
        /// 异步获取Session
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        ValueTask<IEnumerable<IAppSession>> GetSessionsAsync(Predicate<IAppSession> criteria = null);
        /// <summary>
        /// 异步获取Session
        /// </summary>
        /// <typeparam name="TAppSession"></typeparam>
        /// <param name="criteria"></param>
        /// <returns></returns>
        ValueTask<IEnumerable<TAppSession>> GetSessionsAsync<TAppSession>(Predicate<TAppSession> criteria = null)
            where TAppSession : IAppSession;
    }
}