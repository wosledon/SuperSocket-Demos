using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public class SyncToAsyncSessionContainerWrapper : IAsyncSessionContainer
    {
        ISessionContainer _syncSessionContainer;
        /// <summary>
        /// 初始化Session容器
        /// </summary>
        public ISessionContainer SessionContainer
        {
            get { return _syncSessionContainer; }
        }
        /// <summary>
        /// 同步转异步Session容器包装
        /// </summary>
        /// <param name="syncSessionContainer"></param>
        public SyncToAsyncSessionContainerWrapper(ISessionContainer syncSessionContainer)
        {
            _syncSessionContainer = syncSessionContainer;
        }
        /// <summary>
        /// 使用ID异步获取Session
        /// </summary>
        /// <param name="sessionID">要获取Session的ID</param>
        /// <returns>Session实例</returns>
        public ValueTask<IAppSession> GetSessionByIDAsync(string sessionID)
        {
            return new ValueTask<IAppSession>(_syncSessionContainer.GetSessionByID(sessionID));
        }
        /// <summary>
        /// 异步获取Session的数量
        /// </summary>
        /// <returns>Session的数量</returns>
        public ValueTask<int> GetSessionCountAsync()
        {
            return new ValueTask<int>(_syncSessionContainer.GetSessionCount());
        }
        /// <summary>
        /// 异步获取Session
        /// </summary>
        /// <param name="criteria">标准</param>
        /// <returns></returns>
        public ValueTask<IEnumerable<IAppSession>> GetSessionsAsync(Predicate<IAppSession> criteria = null)
        {
            return new ValueTask<IEnumerable<IAppSession>>(_syncSessionContainer.GetSessions(criteria));
        }
        /// <summary>
        /// 异步获取Session
        /// </summary>
        /// <typeparam name="TAppSession">Session的类型</typeparam>
        /// <param name="criteria">标准</param>
        /// <returns></returns>
        public ValueTask<IEnumerable<TAppSession>> GetSessionsAsync<TAppSession>(Predicate<TAppSession> criteria = null) where TAppSession : IAppSession
        {
            return new ValueTask<IEnumerable<TAppSession>>(_syncSessionContainer.GetSessions<TAppSession>(criteria));
        }
    }
}