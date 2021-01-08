using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Models
{
    public class TextMessageModel
    {
        public string LocalName { get; set; } = null;
        public string RemoteName { get; set; } = null;
        public string TextMessage { get; set; } = null;
    }
}
