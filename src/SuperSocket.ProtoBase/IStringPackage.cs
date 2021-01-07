using SuperSocket;

namespace SuperSocket.ProtoBase
{
    public interface IStringPackage
    {
        /// <summary>
        /// 字符串包消息体
        /// </summary>
        string Body { get; set; }
    }
}