using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = System.Drawing.Image;

namespace Chat.UdpDemo
{
    public class UdpTools
    {
        public static int Count = 30000;
        public static List<UdpPackage> Packages = new List<UdpPackage>();
        public UdpTools() { }

        public UdpTools(int count)
        {
            Count = count;
        }
        /// <summary>
        /// 拆包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        public List<byte[]> PackageSlice(byte[] buffer, int identity)
        {
            List<byte[]> result = new List<byte[]>();
            int size = buffer.Length / Count;
            if (buffer.Length % Count > 0)
            {
                size++;
            }

            for (int i = 0; i < size; i++)
            {
                var tmp = buffer.Length - i * Count;
                var slicePackage = new UdpPackage()
                {
                    PackageIdentityNum = identity,
                    PackageSerialNum = i,
                    PackageCount = size,
                    PackageBuffer = buffer.Skip(i * Count).Take(tmp < Count ? tmp : Count).ToArray()
                };
                
                result.Add(UdpTools.Object2Bytes(slicePackage));
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
            var package = (UdpPackage)Bytes2Object(buffer);
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
        public byte[] PackageMerge(List<UdpPackage> packages, int identity)
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
                    buffPackage.PackageBuffer.CopyTo(buffer, buffPackage.PackageSerialNum*Count);
                }
            }
            ClearPackages(identity);
            return buffer;
        }

        private void ClearPackages(int identity)
        {
            Packages = Packages.Where(x => x.PackageIdentityNum != identity).ToList();
        }

        /// <summary>
        /// 将对象转换为byte数组
        /// </summary>
        /// <param name="obj">被转换对象</param>
        /// <returns>转换后byte数组</returns>
        public static byte[] Object2Bytes(UdpPackage obj)
        {
            byte[] buff;
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter iFormatter = new BinaryFormatter();
                iFormatter.Serialize(ms, obj);
                buff = ms.GetBuffer();
            }
            return buff;
        }

        /// <summary>
        /// 将byte数组转换成对象
        /// </summary>
        /// <param name="buff">被转换byte数组</param>
        /// <returns>转换完成后的对象</returns>
        public static UdpPackage Bytes2Object(byte[] buff)
        {
            UdpPackage obj;
            using (MemoryStream ms = new MemoryStream(buff))
            {
                IFormatter iFormatter = new BinaryFormatter();
                obj = (UdpPackage)iFormatter.Deserialize(ms);
            }
            return obj;
        }

        /// <summary>
        /// 将图片以二进制流
        /// </summary>
        /// <param name="path"></param>
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
        /// <param name="imgBytesIn"></param>
        public static BitmapImage BytesToBitmapImage(byte[] buffer)
        {
            BitmapImage bim = new BitmapImage();
            bim.BeginInit();
            bim.StreamSource = new MemoryStream(buffer);
            bim.EndInit();

            return bim;
        }
    }
}