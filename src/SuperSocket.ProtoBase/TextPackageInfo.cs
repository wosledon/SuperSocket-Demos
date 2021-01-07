namespace SuperSocket.ProtoBase
{
    public class TextPackageInfo
    {
        /// <summary>
        /// 文本信息
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 转为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
