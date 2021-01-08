using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chat.Models
{
    public class MessagePackage<TMessage>
    {
        public OpCode OpCode { get; set; } = OpCode.All;
        public MessageType MessageType { get; set; } = MessageType.TextMessage;
        public TMessage Message { get; set; } = default(TMessage);
        //public ClientItems Clients { get; set; } = null;
        public IEnumerable<ClientInfo> Clients { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\r\n";
        }
    }
}
