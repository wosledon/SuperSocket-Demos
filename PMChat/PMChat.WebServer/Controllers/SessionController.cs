using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SuperSocket;
using SuperSocket.Server;

namespace PMChat.WebServer.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public class SessionController: ControllerBase
    {
        private readonly ISessionContainer _sessionContainer;
        
        public SessionController(ISessionContainer sessionContainer)
        {
            _sessionContainer = sessionContainer;
        }

        [HttpGet]
        public int Get()
        {
            var res = _sessionContainer.GetSessionCount();

            return res;
        }
    }
}