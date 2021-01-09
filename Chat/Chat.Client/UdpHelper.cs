using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Chat.Client
{
    public class UdpHelper
    {
        public static int Count { get; set; } = 6000;
        public static List<UdpPackage> udps { get; set; } = new List<UdpPackage>();
        public UdpHelper() { }
        //每个包中二进制数组的长度
        

        //分包的方法，传入一个byte数组和包的编号（编号是用来判断收到数据是否是同一个包的）
        public List<UdpPackage> GetList(byte[] bytes, int num)
        {
            List<UdpPackage> packages = new List<UdpPackage>();//定义一个包集合
            int count = 0;//用来记录包在包集合中的位置
            int nums = bytes.Length / Count;//分包后包集合中包的数量
            if (bytes.Length % Count > 0) nums++;

            //循环遍历传入的byte数组，根据每个包中的二进制数组长度（Count）来分割
            for (int i = 0; i <= bytes.Length; i += Count)
            {
                count++;//位置++

                byte[] bs = new byte[Count];
                int all = Count;
                //判断最后一个分包是否超过剩余byte数组的长度
                //如果超过，则最后一个包的长度为剩余byte数组的长度
                if (i + Count > bytes.Length)
                    all = bytes.Length - i;
                //将传入byte数组copy到包中
                Buffer.BlockCopy(bytes, i, bs, 0, all);
                packages.Add(new UdpPackage
                {
                    Bytes = bs,
                    PacketNum = num,
                    PacketAtNum = count,
                    PacketCount = nums
                });
            }
            return packages;
        }


        //组合包方法，传入一个包集合，和包的编号
        public byte[] GetBytes(UdpPackage package, int count)
        {
            byte[] bytes = null;//接包二进制集合
            udps.Add(package);//把传入的包add进包集合中

            //linq查询，查出包集合中所有和传入包编号拥有相同编号的包，并根据所在包
            //的位置排序
            var result = from n in udps
                where n.PacketNum == count
                orderby n.PacketAtNum
                select n;

            //判断查出包的集合是否跟分包时的数量一样
            if (result.Count() == package.PacketCount)
            {
                bytes = new byte[5120000];//初始化接包二进制数组
                int jiShu = 0;//定义一个计数器

                //遍历所有查询结果中的包，把每个包中的二进制数组组合起来
                foreach (UdpPackage v in result)
                {
                    //把当前循环中包的二进制集合copy到接包数组中，从jishu*Count开始
                    v.Bytes.CopyTo(bytes, jiShu * Count);
                    //计数器++
                    jiShu++;
                }

                //调用清包方法，清除所有组包完成的包
                Clear(count);

            }

            return bytes;
        }

        //清除方法，根据传入的包编号删除所有包集合中拥有此包编号的包
        private void Clear(int index)
        {
            for (int i = 0; i < udps.Count;)
            {
                //判读包的编号是否已传入的编号相同
                //如果相同则删除，否则进入下一个
                if (udps[i].PacketNum == index)
                    udps.Remove(udps[i]);
                else
                    i++;
            }
        }

        /// <summary>
        /// 将对象转换为byte数组
        /// </summary>
        /// <param name="obj">被转换对象</param>
        /// <returns>转换后byte数组</returns>
        public static byte[] Object2Bytes(object obj)
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
        public static object Bytes2Object(byte[] buff)
        {
            object obj;
            using (MemoryStream ms = new MemoryStream(buff))
            {
                IFormatter iFormatter = new BinaryFormatter();
                obj = iFormatter.Deserialize(ms);
            }
            return obj;
        }
    }
}