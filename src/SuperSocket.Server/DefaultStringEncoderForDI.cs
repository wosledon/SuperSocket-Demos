using System;
using System.IO.Pipelines;
using System.Text;
using Microsoft.Extensions.Options;
using SuperSocket.ProtoBase;

namespace SuperSocket
{
    class DefaultStringEncoderForDI : DefaultStringEncoder
    {
        /// <summary>
        /// ³õÊ¼»¯DIÄ¬ÈÏµÄ×Ö·û´®±àÂëÆ÷
        /// </summary>
        /// <param name="serverOptions"></param>
        public DefaultStringEncoderForDI(IOptions<ServerOptions> serverOptions)
            : base(serverOptions.Value?.DefaultTextEncoding ?? new UTF8Encoding(false))
        {

        }
    }
}