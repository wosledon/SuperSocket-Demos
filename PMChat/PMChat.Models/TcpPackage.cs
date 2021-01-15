using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PMChat.Models
{
    public class TcpPackage
    {
        public OpCode OpCode { get; set; } = OpCode.All;
        public string LocalName { get; set; }
        public string RemoteName { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Text;
        public string Message { get; set; } = String.Empty;
        public List<ClientInfo> Clients { get; set; } = null;
        public UdpConfigPackage Config { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\r\n";
        }

        public static TcpPackage JsonToPackage(string data)
        {
            return JsonConvert.DeserializeObject<TcpPackage>(data);
        }
    }
}