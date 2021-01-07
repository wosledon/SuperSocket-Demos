using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    class ClearIdleSessionMiddleware : MiddlewareBase
    {
        private ISessionContainer _sessionContainer;

        private Timer _timer;

        private ServerOptions _serverOptions;

        private ILogger _logger;
        /// <summary>
        /// 初始化清除空闲Session中间件
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="serverOptions"></param>
        /// <param name="loggerFactory"></param>
        public ClearIdleSessionMiddleware(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions, ILoggerFactory loggerFactory)
        {
            _sessionContainer = serviceProvider.GetService<ISessionContainer>();
            
            if (_sessionContainer == null)
                throw new Exception($"{nameof(ClearIdleSessionMiddleware)} needs a middleware of {nameof(ISessionContainer)}");

            _serverOptions = serverOptions.Value;
            _logger = loggerFactory.CreateLogger<ClearIdleSessionMiddleware>();
        }
        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="server"></param>
        public override void Start(IServer server)
        {
            _timer = new Timer(OnTimerCallback, null, _serverOptions.ClearIdleSessionInterval * 1000, _serverOptions.ClearIdleSessionInterval * 1000);
        }
        /// <summary>
        /// 定时回调
        /// </summary>
        /// <param name="state"></param>
        private void OnTimerCallback(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                var timeoutTime = DateTimeOffset.Now.AddSeconds(0 - _serverOptions.IdleSessionTimeOut);

                foreach (var s in _sessionContainer.GetSessions())
                {
                    if (s.LastActiveTime <= timeoutTime)
                    {
                        try
                        {
                            s.Channel.CloseAsync(CloseReason.TimeOut);
                            _logger.LogWarning($"Close the idle session {s.SessionID}, it's LastActiveTime is {s.LastActiveTime}.");
                        }
                        catch (Exception exc)
                        {
                            _logger.LogError(exc, $"Error happened when close the session {s.SessionID} for inactive for a while.");
                        }                        
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error happened when clear idle session.");
            }

            _timer.Change(_serverOptions.ClearIdleSessionInterval * 1000, _serverOptions.ClearIdleSessionInterval * 1000);
        }
        /// <summary>
        /// 超时停止
        /// </summary>
        /// <param name="server"></param>
        public override void Shutdown(IServer server)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();
            _timer = null;
        }
    }
}