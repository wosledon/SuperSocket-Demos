using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    interface IWebSocketServerMiddleware
    {
        ValueTask HandleSessionHandshakeCompleted(WebSocketSession session);
    }

    class WebSocketServerMiddleware : MiddlewareBase, IWebSocketServerMiddleware
    {
        private ConcurrentQueue<WebSocketSession> _openHandshakePendingQueue = new ConcurrentQueue<WebSocketSession>();

        private ConcurrentQueue<WebSocketSession> _closeHandshakePendingQueue = new ConcurrentQueue<WebSocketSession>();
        
        private Timer _checkingTimer;

        private readonly HandshakeOptions _options;

        private IMiddleware _sessionContainerMiddleware;

        private ISessionEventHost _sessionEventHost;
        /// <summary>
        /// 初始化WebSocket中间件
        /// </summary>
        /// <param name="handshakeOptions">握手设置</param>
        public WebSocketServerMiddleware(IOptions<HandshakeOptions> handshakeOptions)
        {
            var options = handshakeOptions.Value;

            if (options == null)
                options = new HandshakeOptions();

            _options = options;        
        }
        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="server">服务器</param>
        public override void Start(IServer server)
        {
            _sessionContainerMiddleware = server.GetSessionContainer() as IMiddleware;
            _sessionEventHost = server as ISessionEventHost;
            _checkingTimer = new Timer(HandshakePendingQueueCheckingCallback, null, _options.CheckingInterval * 1000, _options.CheckingInterval * 1000); // hardcode to 1 minute for now
        }
        /// <summary>
        /// 结束
        /// </summary>
        /// <param name="server">服务器</param>
        public override void Shutdown(IServer server)
        {
            _checkingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _checkingTimer.Dispose();
            _checkingTimer = null;
            _sessionContainerMiddleware = null;
        }
        /// <summary>
        /// 注册Session
        /// </summary>
        /// <param name="session">Session</param>
        /// <returns></returns>
        public override ValueTask<bool> RegisterSession(IAppSession session)
        {
            var websocketSession = session as WebSocketSession;
            _openHandshakePendingQueue.Enqueue(websocketSession);
            websocketSession.CloseHandshakeStarted += OnCloseHandshakeStarted;            
            return new ValueTask<bool>(true);
        }
        /// <summary>
        /// 关闭握手开始事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCloseHandshakeStarted(object sender, EventArgs e)
        {
            var session = sender as WebSocketSession;
            session.CloseHandshakeStarted -= OnCloseHandshakeStarted;
            _closeHandshakePendingQueue.Enqueue(session);
        }
        /// <summary>
        /// 握手等待队列检查回调
        /// </summary>
        /// <param name="state">状态</param>
        private void HandshakePendingQueueCheckingCallback(object state)
        {
            _checkingTimer.Change(Timeout.Infinite, Timeout.Infinite);

            while (true)
            {
                WebSocketSession session;

                if (!_openHandshakePendingQueue.TryPeek(out session))
                    break;

                if (session.Handshaked || session.State == SessionState.Closed)
                {
                    //Handshaked or not connected
                    _openHandshakePendingQueue.TryDequeue(out session);
                    continue;
                }

                if (DateTime.Now < session.StartTime.AddSeconds(_options.OpenHandshakeTimeOut))
                    break;

                //Timeout, dequeue and then close
                _openHandshakePendingQueue.TryDequeue(out session);
                session.CloseWithoutHandshake();
            }

            while (true)
            {
                WebSocketSession session;

                if (!_closeHandshakePendingQueue.TryPeek(out session))
                    break;

                if (session.State == SessionState.Closed)
                {
                    //the session has been closed
                    _closeHandshakePendingQueue.TryDequeue(out session);
                    continue;
                }

                if (DateTime.Now < session.CloseHandshakeStartTime.AddSeconds(_options.CloseHandshakeTimeOut))
                    break;

                //Timeout, dequeue and then close
                _closeHandshakePendingQueue.TryDequeue(out session);
                //Needn't send closing handshake again
                session.CloseWithoutHandshake();
            }

            _checkingTimer?.Change(_options.CheckingInterval * 1000, _options.CheckingInterval * 1000);
        }
        /// <summary>
        /// 处理Session握手完成后
        /// </summary>
        /// <param name="session">Session</param>
        /// <returns></returns>
        public ValueTask HandleSessionHandshakeCompleted(WebSocketSession session)
        {
            _sessionContainerMiddleware?.RegisterSession(session);
            return _sessionEventHost.HandleSessionConnectedEvent(session);
        }
    }
}