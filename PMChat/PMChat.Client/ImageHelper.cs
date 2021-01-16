using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace PMChat.Client
{
    public class ImageHelper
    {
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
    }
}