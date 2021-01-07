using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{
public class TransparentPipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo>
where TPackageInfo : class
{
/// <summary>
/// É¸Ñ¡Æ÷
/// </summary>
/// <param name="reader">ÐòÁÐÔÄ¶ÁÆ÷</param>
/// <returns></returns>
public override TPackageInfo Filter(ref SequenceReader<byte> reader)
{
    var sequence = reader.Sequence;
    var total = reader.Remaining;
    var package = DecodePackage(ref sequence);
    reader.Advance(total);
    return package;
}
}
}
