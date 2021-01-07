using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    class DefaultSessionFactory : ISessionFactory
    {
        public Type SessionType => typeof(AppSession);
        /// <summary>
        /// ´´½¨Session
        /// </summary>
        /// <returns></returns>
        public IAppSession Create()
        {
            return new AppSession();
        }
    }
}