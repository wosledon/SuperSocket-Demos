using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    public class AppSession : IAppSession, ILogger, ILoggerAccessor
    {
        private IChannel _channel;

        protected internal IChannel Channel
        {
            get { return _channel; }
        }

        public AppSession()
        {
            
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="server">服务器</param>
        /// <param name="channel">通道</param>
        void IAppSession.Initialize(IServerInfo server, IChannel channel)
        {
            if (channel is IChannelWithSessionIdentifier channelWithSessionIdentifier)
                SessionID = channelWithSessionIdentifier.SessionIdentifier;
            else                
                SessionID = Guid.NewGuid().ToString();
            
            Server = server;
            StartTime = DateTimeOffset.Now;
            _channel = channel;
            State = SessionState.Initialized;
        }

        public string SessionID { get; private set; }

        public DateTimeOffset StartTime { get; private set; }

        public SessionState State { get; private set; } = SessionState.None;

        public IServerInfo Server { get; private set; }

        IChannel IAppSession.Channel
        {
            get { return _channel; }
        }

        public object DataContext { get; set; }

        public EndPoint RemoteEndPoint
        {
            get { return _channel?.RemoteEndPoint; }
        }

        public EndPoint LocalEndPoint
        {
            get { return _channel?.LocalEndPoint; }
        }

        public DateTimeOffset LastActiveTime
        {
            get { return _channel?.LastActiveTime ?? DateTimeOffset.MinValue; }
        }

        public event AsyncEventHandler Connected;

        public event AsyncEventHandler<CloseEventArgs> Closed;
        
        private Dictionary<object, object> _items;

        public object this[object name]
        {
            get
            {
                var items = _items;

                if (items == null)
                    return null;

                object value;
                
                if (items.TryGetValue(name, out value))
                    return value;

                return null;
            }

            set
            {
                lock (this)
                {
                    var items = _items;

                    if (items == null)
                        items = _items = new Dictionary<object, object>();

                    items[name] = value;
                }
            }
        }
        /// <summary>
        /// Session关闭后
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual ValueTask OnSessionClosedAsync(CloseEventArgs e)
        {
            return new ValueTask();
        }
        /// <summary>
        /// 异步释放Session关闭
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        internal async ValueTask FireSessionClosedAsync(CloseEventArgs e)
        {
            State = SessionState.Closed;

            await OnSessionClosedAsync(e);

            var closeEventHandler = Closed;

            if (closeEventHandler == null)
                return;

             await closeEventHandler.Invoke(this, e);
        }

        /// <summary>
        /// Session连接后
        /// </summary>
        /// <returns></returns>
        protected virtual ValueTask OnSessionConnectedAsync()
        {
            return new ValueTask();
        }
        /// <summary>
        /// 释放Session连接后
        /// </summary>
        /// <returns></returns>
        internal async ValueTask FireSessionConnectedAsync()
        {
            State = SessionState.Connected;

            await OnSessionConnectedAsync();            

            var connectedEventHandler = Connected;

            if (connectedEventHandler == null)
                return;

            await connectedEventHandler.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        ValueTask IAppSession.SendAsync(ReadOnlyMemory<byte> data)
        {
            return _channel.SendAsync(data);
        }
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <typeparam name="TPackage">包的类型</typeparam>
        /// <param name="packageEncoder">包的编码</param>
        /// <param name="package">数据包</param>
        /// <returns></returns>
        ValueTask IAppSession.SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            return _channel.SendAsync(packageEncoder, package);
        }
        /// <summary>
        /// 重置
        /// </summary>
        void IAppSession.Reset()
        {
            ClearEvent(ref Connected);
            ClearEvent(ref Closed);
            _items?.Clear();
            State = SessionState.None;
            _channel = null;
            DataContext = null;
            StartTime = default(DateTimeOffset);
            Server = null;

            Reset();
        }

        protected virtual void Reset()
        {

        }
        /// <summary>
        /// 清除事件
        /// </summary>
        /// <typeparam name="TEventHandler"></typeparam>
        /// <param name="sessionEvent"></param>
        private void ClearEvent<TEventHandler>(ref TEventHandler sessionEvent)
            where TEventHandler : Delegate
        {
            if (sessionEvent == null)
                return;

            foreach (var handler in sessionEvent.GetInvocationList())
            {
                sessionEvent = Delegate.Remove(sessionEvent, handler) as TEventHandler;
            }
        }
        /// <summary>
        /// 异步关闭
        /// </summary>
        /// <returns></returns>
        public virtual async ValueTask CloseAsync()
        {
            await CloseAsync(CloseReason.LocalClosing);
        }
        /// <summary>
        /// 异步关闭
        /// </summary>
        /// <param name="reason">关闭原因</param>
        /// <returns></returns>
        public virtual async ValueTask CloseAsync(CloseReason reason)
        {
            var channel = Channel;

            if (channel == null)
                return;
            
            try
            {
                await channel.CloseAsync(reason);
            }
            catch
            {
            }
        }

        #region ILogger

        ILogger GetLogger()
        {
            return (Server as ILoggerAccessor).Logger;
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            GetLogger().Log<TState>(logLevel, eventId, state, exception, (s, e) =>
            {
                return $"Session[{this.SessionID}]: {formatter(s, e)}";
            });
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return GetLogger().IsEnabled(logLevel);
        }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return GetLogger().BeginScope<TState>(state);
        }

        public ILogger Logger => this as ILogger;

        #endregion
    }
}