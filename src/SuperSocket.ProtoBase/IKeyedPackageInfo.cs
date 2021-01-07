using System;

namespace SuperSocket.ProtoBase
{
    public interface IKeyedPackageInfo<TKey>
    {
        /// <summary>
        /// 键值
        /// </summary>
        TKey Key { get; }
    }
}
