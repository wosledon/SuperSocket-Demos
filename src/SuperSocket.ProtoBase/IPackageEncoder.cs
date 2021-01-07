using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public interface IPackageEncoder<in TPackageInfo>
    {
        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="writer">字节码写入器</param>
        /// <param name="pack">包信息</param>
        /// <returns></returns>
        int Encode(IBufferWriter<byte> writer, TPackageInfo pack);
    }
}