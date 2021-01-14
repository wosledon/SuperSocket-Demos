using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    /// <summary>
    /// 文件切块
    /// 块数据切片
    /// 收集切片
    /// 切片组块
    /// 块写入文件
    /// </summary>
    public interface IUdpPackageManager
    {
        /// <summary>
        /// 块大小
        /// </summary>
        int BlockSize { get; set; }
        /// <summary>
        /// 切片大小
        /// </summary>
        ushort SliceSize { get; set; }
        /// <summary>
        /// 读取文件成块
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>块转成字节数组</returns>
        Task<byte[]> ReadFileToBlock(string filePath);
        /// <summary>
        /// 块切片
        /// </summary>
        /// <param name="buffer">块的字节数组</param>
        /// <param name="identity">文件标识</param>
        /// <returns>发送队列</returns>
        Task<IQueueManager> BlockToSlice(byte[] buffer, byte identity);

        /// <summary>
        /// 收集接收到的切片
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="lostSlices"></param>
        /// <param name="transferFinished"></param>
        /// <returns></returns>
        bool CollectSlices(byte[] buffer, out ushort[] lostSlices, out bool transferFinished);
        bool BlockIntegrityChecks(ushort blockSerial, out ushort[] lostSlices, out bool transferFinished);
        Task<byte[]> SliceToBlock(List<UdpPackage> slices, byte identity);
        Task<bool> WriteBlockToFile(string filePath, byte[] buffer);

        void NextBlock();
        void ClearOneBlockSlices(byte identity, ushort blockSerial);
    }

    public interface IUdpPackageManager<TPackage>
    {
        int BlockSize { get; set; }
        int SliceSize { get; set; }
        Task<byte[]> ReadFileToBlock(string filePath);
        Task<IQueueManager<TPackage>> BlockToSlice(byte[] buffer);
        Task<byte[]> SliceToBlock(IQueueManager<TPackage> slices);
        Task<bool> WriteBlockToFile(byte[] buffer);
    }
}