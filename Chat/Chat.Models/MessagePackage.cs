using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Models
{
    public class MessagePackage
    {
        public OpCode OpCode { get; set; } = OpCode.All;
        public MessageType MessageType { get; set; } = MessageType.TextMessage;
        public object Message { get; set; }
    }
}
