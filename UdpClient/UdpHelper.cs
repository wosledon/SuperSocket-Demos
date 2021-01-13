using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UdpClient
{
    public class UdpHelper
    {
        public static int Count { get; set; } = 60000;
        //{
        //    set
        //    {
        //        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        //        Count = value > 60000 ? 60000 : value;
        //    }

        //    get => Count == default ? 60000 : Count;
        //}
        public static List<UdpPackage> Packages = new List<UdpPackage>();

        public UdpHelper() { }

        public UdpHelper(int count)
        {
            Count = count;
        }

        /// <summary>
        /// 拆包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="identity"></param>
        /// <param name="fileModel"></param>
        /// <returns></returns>
        public List<byte[]> PackageSlice(byte[] buffer, ushort identity, UdpFileModel fileModel)
        {
            List<byte[]> result = new List<byte[]>();
            ushort size = (ushort)(buffer.Length / Count);
            if (buffer.Length % Count > 0)
            {
                size++;
            }

            for (ushort i = 0; i < size; i++)
            {
                var tmp = buffer.Length - i * Count;
                var slicePackage = new UdpPackage()
                {
                    PackageIdentityNum = identity,
                    PackageSerialNum = i,
                    PackageFileMode = fileModel,
                    PackageCount = size,
                    PackageBuffer = buffer.Skip(i * Count).Take(tmp < Count ? tmp : Count).ToArray()
                };

                result.Add(UdpPackageToBytes(slicePackage));
            }

            return result;
        }
        /// <summary>
        /// 收集包
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int PackageConcat(byte[] buffer)
        {
            if (buffer.Length == 0)
            {
                return -1;
            }
            var package = (UdpPackage)BytesToUdpPackage(buffer);
            Packages.Add(package);
            if (package.PackageCount == Packages.Count)
            {
                return 1;
            }

            return 0;
        }
        /// <summary>
        /// 组包
        /// </summary>
        /// <param name="packages"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        public byte[] PackageMerge(List<UdpPackage> packages, ushort identity)
        {
            var result =
                from package in packages
                where package.PackageIdentityNum == identity
                orderby package.PackageSerialNum
                select package;
            var size = result.Sum(x => x.PackageBuffer.Length);

            byte[] buffer = null;
            var packageSize = packages.First().PackageCount;
            if (result.Count() == packageSize)
            {
                buffer = new byte[size];
                foreach (var buffPackage in result)
                {
                    buffPackage.PackageBuffer.CopyTo(buffer, buffPackage.PackageSerialNum * Count);
                }
            }
            ClearPackages(identity);
            return buffer;
        }

        public static void ClearPackages(int identity)
        {
            Packages = Packages.Where(x => x.PackageIdentityNum != identity).ToList();
        }

        /// <summary>
        /// Udp数据包转byte数组
        /// </summary>
        /// <param name="package">udp数据包</param>
        /// <returns></returns>
        public static byte[] UdpPackageToBytes(UdpPackage package)
        {
            byte[] buffer = new byte[package.PackageBuffer.Length+8];
            BitConverter.GetBytes(package.PackageIdentityNum).CopyTo(buffer, 0);
            BitConverter.GetBytes(package.PackageSerialNum).CopyTo(buffer, 2);
            BitConverter.GetBytes((ushort)package.PackageFileMode).CopyTo(buffer, 4);
            BitConverter.GetBytes(package.PackageCount).CopyTo(buffer, 6);
            package.PackageBuffer.CopyTo(buffer, 8);


            return buffer;
        }
        /// <summary>
        /// byte数组转udp数据包
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static UdpPackage BytesToUdpPackage(byte[] buffer)
        {
            return new UdpPackage()
            {
                PackageIdentityNum = BitConverter.ToUInt16(buffer, 0),
                PackageSerialNum = BitConverter.ToUInt16(buffer, 2),
                PackageFileMode = (UdpFileModel)BitConverter.ToInt16(buffer, 4),
                PackageCount = BitConverter.ToUInt16(buffer, 6),
                PackageBuffer = buffer.Skip(8).ToArray()
            };
        }

        /// <summary>
        /// 将图片以二进制流
        /// </summary>
        /// <param name="path">图片的路径</param>
        /// <returns></returns>
        public static byte[] ImageToBytes(String path)
        {
            using (BinaryReader loader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                FileInfo fd = new FileInfo(path);
                int Length = (int)fd.Length;
                byte[] buf = new byte[Length];
                buf = loader.ReadBytes((int)fd.Length);
                loader.Dispose();
                loader.Close();

                return buf;
            }
        }


        /// <summary>
        /// 显示二进制流代表的图片
        /// </summary>
        /// <param name="buffer"></param>
        public static BitmapImage BytesToBitmapImage(byte[] buffer)
        {
            BitmapImage bim = new BitmapImage();
            bim.BeginInit();
            bim.StreamSource = new MemoryStream(buffer);
            bim.EndInit();
            

            return bim;
        }

        /// <summary>
        /// 文件转二进制
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件的二进制文件</returns>
        public static byte[] FileToBytes(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return new byte[0];
            }

            FileInfo fi = new FileInfo(filePath);
            byte[] buffer = new byte[fi.Length];

            FileStream fs = fi.OpenRead();
            fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
            fs.Close();

            return buffer;
        }
        /// <summary>
        /// byte数组转文件并保存
        /// </summary>
        /// <param name="buffer">文件的byte数组形式</param>
        /// <param name="savePath">保存的路径</param>
        public static void BytesToFile(byte[] buffer, string savePath)
        {
            if (System.IO.File.Exists(savePath))
            {
                System.IO.File.Delete(savePath);
            }

            using (FileStream fs = new FileStream(savePath, FileMode.CreateNew))
            {
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(buffer, 0, buffer.Length);
                bw.Close();
                fs.Close();
            }
        }

        /// <summary>
        /// 将文件转换为byte数组
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <returns>转换后的byte数组</returns>
        public static byte[] File2Bytes(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return new byte[0];
            }

            FileInfo fi = new FileInfo(path);
            byte[] buff = new byte[fi.Length];

            FileStream fs = fi.OpenRead();
            fs.Read(buff, 0, Convert.ToInt32(fs.Length));
            fs.Close();

            return buff;
        }

        /// <summary>
        /// 将byte数组转换为文件并保存到指定地址
        /// </summary>
        /// <param name="buff">byte数组</param>
        /// <param name="savepath">保存地址</param>
        public static void Bytes2File(byte[] buff, string savepath)
        {
            if (System.IO.File.Exists(savepath))
            {
                System.IO.File.Delete(savepath);
            }

            FileStream fs = new FileStream(savepath, FileMode.CreateNew);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(buff, 0, buff.Length);
            bw.Close();
            fs.Close();
        }
    }
}