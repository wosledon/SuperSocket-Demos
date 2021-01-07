using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public class AsyncToSyncSessionContainerWrapper : ISessionContainer
    {
        IAsyncSessionContainer _asyncSessionContainer;
        /// <summary>
        /// 异步转同步Session容器包装
        /// </summary>
        /// <param name="asyncSessionContainer">异步Session容器</param>
        public AsyncToSyncSessionContainerWrapper(IAsyncSessionContainer asyncSessionContainer)
        {
            _asyncSessionContainer = asyncSessionContainer;
        }
        /// <summary>
        /// 通过ID获取Session
        /// </summary>
        /// <param name="sessionID">要获取的SessionID</param>
        /// <returns></returns>
        public IAppSession GetSessionByID(string sessionID)
        {
            return _asyncSessionContainer.GetSessionByIDAsync(sessionID).Result;
        }
        /// <summary>
        /// 获取Session个数
        /// </summary>
        /// <returns>Session的个数</returns>
        public int GetSessionCount()
        {
            return _asyncSessionContainer.GetSessionCountAsync().Result;
        }
        /// <summary>
        /// 获取Session
        /// </summary>
        /// <param name="criteria">标准</param>
        /// <returns></returns>
        public IEnumerable<IAppSession> GetSessions(Predicate<IAppSession> criteria)
        {
            return _asyncSessionContainer.GetSessionsAsync(criteria).Result;
        }
        /// <summary>
        /// 获取Session
        /// </summary>
        /// <typeparam name="TAppSession">Session</typeparam>
        /// <param name="criteria">标准</param>
        /// <returns></returns>
        public IEnumerable<TAppSession> GetSessions<TAppSession>(Predicate<TAppSession> criteria) where TAppSession : IAppSession
        {
            return _asyncSessionContainer.GetSessionsAsync<TAppSession>(criteria).Result;
        }
    }
}