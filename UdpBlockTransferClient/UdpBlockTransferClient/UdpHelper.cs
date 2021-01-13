using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace UdpBlockTransferClient
{
    public class UdpHelper
    {
        public int SlicePackageSize { get; set; } = 1024 * 60;
        //{
        //    // ReSharper disable once FunctionRecursiveOnAllPaths
        //    get => SlicePackageSize;
        //    set
        //    {
        //        //if (value < 0 || value > 1400)
        //        //{
        //        //    SlicePackageSize = 1024;
        //        //}
        //        //else
        //        //{
        //        //    SlicePackageSize = value;
        //        //}
        //        SlicePackageSize = value;
        //    }
        //}

        public int BlockSize { get; set; } = 1024 * 1024;
        //{
        //    // ReSharper disable once FunctionRecursiveOnAllPaths
        //    get => BlockSize;
        //    set
        //    {
        //        if (value < 0 || value > 50 * 1024 * 1024)
        //        {
        //            BlockSize = 1024 * 1024;
        //        }
        //        else
        //        {
        //            BlockSize = value;
        //        }
        //        BlockSize = value;
        //    }
        //}

        public ushort BlockSerial { get; private set; } = 1;

        private string FileName { get; set; } = "Default";

        public List<UdpPackage> BlockSlicePackages = new List<UdpPackage>();

        public UdpHelper()
        {
            BlockSize = 1024 * 1024;
            SlicePackageSize = 1024;
        }
        /// <summary>
        /// 设置切片大小
        /// </summary>
        /// <param name="slicePackageSize"></param>
        public UdpHelper(int slicePackageSize)
        {
            SlicePackageSize = slicePackageSize;
        }

        public UdpHelper(string filePath)
        {
            SetFileName(filePath);
        }

        /// <summary>
        ///  发送第一块
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public byte[] StartBlock(ushort identity)
        {
            return new UdpPackage()
            {
                PackageIdentity = identity,
                PackageOpCode = (byte)OpCode.Start,
                PackageBody = Encoding.UTF8.GetBytes(FileName)
            }.ToBytes();
        }
        /// <summary>
        /// 发送最后一块
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public byte[] EndBlock(ushort identity)
        {
            return new UdpPackage()
            {
                PackageIdentity = identity,
                PackageOpCode = (byte)OpCode.End,
                PackageBody = Encoding.UTF8.GetBytes(BlockSerial.ToString())
            }.ToBytes();
        }

        /// <summary>
        /// 文件切块
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>切块后返回字节数组</returns>
        public async Task<byte[]> FileDicedBytes(string filePath)
        {
            byte[] data;

            await using (FileStream fileRead = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                long temp = fileRead.Length - BlockSize * (BlockSerial-1);
                if (temp <= 0)
                {
                    return null;
                }
                data = new byte[temp < BlockSize ? temp:BlockSize];
                fileRead.Seek((BlockSerial-1) * BlockSize, SeekOrigin.Begin);
                fileRead.Read(data, 0, (int)(temp < BlockSize ? temp : BlockSize));
                fileRead.Close();
                await fileRead.DisposeAsync();
            }

            return data;
        }
        /// <summary>
        /// 设置文件名
        /// </summary>
        /// <param name="filePath">文件的路径</param>
        private void SetFileName(string filePath)
        {
            //FileName = string.Format($"{Path.GetFileName(filePath)}.{Path.GetExtension(filePath)}");
            FileName = string.Format($"{Path.GetFileName(filePath)}");
        }

        /// <summary>
        /// 将接收到的数据写入文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public async void AppendDataToFile(string filePath, byte[] buffer)
        {
            await using (FileStream fileSave1 = new FileStream($"{filePath}\\{FileName}", FileMode.Append, FileAccess.Write))
            {
                fileSave1.Write(buffer, 0, buffer.Length);
                fileSave1.Flush();
                fileSave1.Close();
                await fileSave1.DisposeAsync();
            }
        }

        /// <summary>
        /// 对块进行切片
        /// </summary>
        /// <param name="buffer">块数据</param>
        /// <param name="identity">文件标识</param>
        /// <returns></returns>
        public List<byte[]> BlockSlice(byte[] buffer, ushort identity)
        {
            List<byte[]> result = new List<byte[]>();
            ushort sliceCount = (ushort)(buffer.Length / SlicePackageSize);
            if (buffer.Length % SlicePackageSize > 0)
            {
                sliceCount++;
            }

            for (ushort i = 1; i <= sliceCount; i++)
            {
                var tmp = buffer.Length - (i-1) * SlicePackageSize;
                var blockSlicePackage = new UdpPackage()
                {
                    PackageIdentity = identity,
                    PackageOpCode = (byte)OpCode.Message,
                    PackageBlockSerial = BlockSerial,
                    PackageSliceSerial = i,
                    PackageSliceCount = sliceCount,
                    PackageBody = buffer.Skip((i-1) * SlicePackageSize)
                            .Take(tmp < SlicePackageSize ? tmp : SlicePackageSize).ToArray()
                };

                result.Add(blockSlicePackage.ToBytes());
            }

            NextBlock();

            return result;
        }

        /// <summary>
        /// 收集块切片
        /// </summary>
        /// <param name="buffer">切片</param>
        /// <param name="lostSlices">丢失的切片序号</param>
        /// <returns>切片组装状态</returns>
        public bool SlicePackageConcat(byte[] buffer,out ushort[] lostSlices)
        {
            lostSlices = null;
            if (buffer.Length == 0)
            {
                return false;
            }

            var package = new UdpPackage().BytesToUdpPackage(buffer);

            switch ((OpCode)package.PackageOpCode)
            {
                case OpCode.Start:
                    FileName = Encoding.UTF8.GetString(package.PackageBody);
                    return false;
                case OpCode.End:
                    return IntegrityChecks(package.PackageSliceCount, out lostSlices);
                case OpCode.Notice:
                    BlockSize = Convert.ToInt32(Encoding.UTF8.GetString(package.PackageBody));
                    return false;
            }

            if (package.PackageSliceCount == package.PackageSliceSerial)
            {
                return true;
            }

            BlockSlicePackages.Add(package);

            return false;
        }

        /// <summary>
        /// 块完整性检查
        /// </summary>
        /// <param name="block"></param>
        /// <param name="lostSlices">返回数组第0为是块号</param>
        /// <returns></returns>
        public bool IntegrityChecks(ushort block, out ushort[] lostSlices)
        {
            if (Math.Abs(block - BlockSerial) == 1)
            {
                lostSlices = null;
                return true;
            }

            var res = BlockSlicePackages.Where(x => x.PackageBlockSerial == block)
                .Where(x => Enumerable.Range(1, block).Contains(x.PackageBlockSerial)).ToList();
            var temp = res.Select(x => x.PackageSliceSerial).ToList();
            temp.Insert(BlockSerial, 0);
            lostSlices = temp.ToArray();
            return false;
        }

        /// <summary>
        /// 切片组装成块
        /// </summary>
        /// <param name="packages"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        public byte[] SlicePackageMergeToBlock(List<UdpPackage> packages, ushort identity)
        {
            var result =
                from package in packages
                where package.PackageIdentity == identity && package.PackageBlockSerial == BlockSerial
                orderby package.PackageSliceSerial
                select package;
            var size = result.Sum(x => x.PackageBody.Length);

            byte[] buffer = null;
            var packageSize = packages.First().PackageSliceCount;
            if (result.Count() == (packageSize-1))
            {
                buffer = new byte[size];
                foreach (var buffPackage in result)
                {
                    buffPackage.PackageBody.CopyTo(buffer, (buffPackage.PackageSliceSerial-1) * SlicePackageSize);
                }
            }
            // 清除当前块的数据
            ClearSlicePackages(identity, BlockSerial);
            // 下一个块
            NextBlock();
            return buffer;
        }
        /// <summary>
        /// 下一个块
        /// </summary>
        public void NextBlock()
        {
            BlockSerial++;
        }
        /// <summary>
        /// 清除已经组块完成的数据
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="block"></param>
        public void ClearSlicePackages(int identity, int block)
        {
            BlockSlicePackages =
                BlockSlicePackages.Where(x => x.PackageIdentity != identity && x.PackageBlockSerial != block).ToList();
        }
    }
}