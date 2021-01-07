using SuperSocket;

namespace SuperSocket.ProtoBase
{
    public class StringPackageInfo : IKeyedPackageInfo<string>, IStringPackage
    {
        /// <summary>
        /// 键值
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 消息体
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public string[] Parameters { get; set; }
    }
}