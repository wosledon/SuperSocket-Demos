using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Channel;

namespace SuperSocket.SessionContainer
{
    public class InProcSessionContainerMiddleware : MiddlewareBase, ISessionContainer
    {
        private ConcurrentDictionary<string, IAppSession> _sessions;
        /// <summary>
        /// 初始化Session容器中间件
        /// </summary>
        /// <param name="serviceProvider">供应商服务</param>
        public InProcSessionContainerMiddleware(IServiceProvider serviceProvider)
        {
            Order = int.MaxValue; // make sure it is the last middleware
            _sessions = new ConcurrentDictionary<string, IAppSession>(StringComparer.OrdinalIgnoreCase);
        }
        /// <summary>
        /// 注册Session
        /// </summary>
        /// <param name="session">Session</param>
        /// <returns>注册结果；true:成功；false:失败；</returns>
        public override ValueTask<bool> RegisterSession(IAppSession session)
        {
            if (session is IHandshakeRequiredSession handshakeSession)
            {
                if (!handshakeSession.Handshaked)
                    return new ValueTask<bool>(true);
            }
            
            session.Closed += OnSessionClosed;
            _sessions.TryAdd(session.SessionID, session);
            return new ValueTask<bool>(true);
        }
        /// <summary>
        /// Session关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private ValueTask OnSessionClosed(object sender, EventArgs e)
        {
            var session  = (IAppSession)sender;

            session.Closed -= OnSessionClosed;
            _sessions.TryRemove(session.SessionID, out IAppSession removedSession);
            
            return new ValueTask();
        }
        /// <summary>
        /// 通过ID获取Session
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns>Session值</returns>
        public IAppSession GetSessionByID(string sessionID)
        {
            _sessions.TryGetValue(sessionID, out IAppSession session);
            return session;
        }
        /// <summary>
        /// 获取Session的数量
        /// </summary>
        /// <returns></returns>
        public int GetSessionCount()
        {
            return _sessions.Count;
        }
        /// <summary>
        /// 获取所有Session
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IEnumerable<IAppSession> GetSessions(Predicate<IAppSession> criteria = null)
        {
            var enumerator = _sessions.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var s = enumerator.Current.Value;

                if (s.State != SessionState.Connected)
                    continue;

                if(criteria == null || criteria(s))
                    yield return s;
            }
        }
        /// <summary>
        /// 获取所有的Session
        /// </summary>
        /// <typeparam name="TAppSession"></typeparam>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IEnumerable<TAppSession> GetSessions<TAppSession>(Predicate<TAppSession> criteria = null) where TAppSession : IAppSession
        {
            var enumerator = _sessions.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value is TAppSession s)
                {
                    if (s.State != SessionState.Connected)
                        continue;
                        
                    if (criteria == null || criteria(s))
                        yield return s;
                }
            }
        }
    }
}
