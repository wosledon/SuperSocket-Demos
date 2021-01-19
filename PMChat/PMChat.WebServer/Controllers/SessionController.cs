using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.Server;

namespace PMChat.WebServer.Controllers
{
    [ApiController]
    [Route("api")]
    public class SessionController : ControllerBase
    {
        private ISessionContainer _session;

        public SessionController(ISessionContainer session)
        {
            _session = session;
        }

        public int Get()
        {
            var res = FinalValues.SessionCount;
            
            return res;
        }
    }
}