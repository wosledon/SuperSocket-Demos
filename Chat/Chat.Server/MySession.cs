using SuperSocket.Server;

namespace Chat.Server
{
    class MySession: AppSession
    {
        public string UserName { get; set; }
    }
}
