using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace PMChat.Models
{
    public class UdpConfigPackage
    {
        public string SendEndPoint { get; set; }
        public string ReceiveEndPoint { get; set; }
    }
}