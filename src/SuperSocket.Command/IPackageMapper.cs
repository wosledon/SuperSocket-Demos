using System;
using System.Threading.Tasks;

namespace SuperSocket.Command
{
    public interface IPackageMapper<PackageFrom, PackageTo>
    {
        /// <summary>
        /// 数据包映射
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        PackageTo Map(PackageFrom package);
    }
}
