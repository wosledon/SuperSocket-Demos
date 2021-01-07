using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket
{
    public class DefaultStringEncoder : IPackageEncoder<string>
    {
        private Encoding _encoding;
        /// <summary>
        /// 实例化默认字符串编码
        /// </summary>
        public DefaultStringEncoder()
            : this(new UTF8Encoding(false))
        {

        }
        /// <summary>
        /// 实例化默认字符串编码
        /// </summary>
        /// <param name="encoding">编码格式</param>
        public DefaultStringEncoder(Encoding encoding)
        {
            _encoding = encoding;
        }
        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="writer">Buffer写入器</param>
        /// <param name="pack">字符串数据</param>
        /// <returns></returns>
        public int Encode(IBufferWriter<byte> writer, string pack)
        {
            return writer.Write(pack, _encoding);
        }
    }
}