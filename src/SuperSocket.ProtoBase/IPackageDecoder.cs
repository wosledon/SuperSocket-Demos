using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public interface IPackageDecoder<out TPackageInfo>
    {
        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer">字节码</param>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        TPackageInfo Decode(ref ReadOnlySequence<byte> buffer, object context);
    }
}