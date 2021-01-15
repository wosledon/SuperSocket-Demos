using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public class UdpPackageManager: IUdpPackageManager
    {
        /// <summary>
        /// 最大块大小:  10MB
        /// </summary>
        private const int MaxBlockSize = 10 * 1024 * 1024;
        /// <summary>
        /// 最大切片大小: 65000B
        /// </summary>
        private const ushort MaxSliceSize = 65000;

        /// <summary>
        /// 数据头长度
        /// </summary>
        public ushort HeaderSize { get; set; } = 10;
        /// <summary>
        /// 块大小:    默认5MB
        /// </summary>
        public int BlockSize { get; set; } = 5 * 1024 * 1024;
        /// <summary>
        /// 切片大小:   默认1KB
        /// </summary>
        public ushort SliceSize { get; set; } = 1024;
        /// <summary>
        /// 块序号
        /// </summary>
        public ushort BlockSerial { get; private set; } = 0;
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName;
        /// <summary>
        /// 接收的片集合
        /// </summary>
        private List<UdpPackage> _slicesList = new List<UdpPackage>();

        /// <summary>
        /// 初始化包管理器
        /// </summary>
        /// <param name="headerSize">数据头长度</param>
        /// <param name="filePath"></param>
        public UdpPackageManager(ushort headerSize, string filePath)
        {
            HeaderSize = headerSize;
            FileName = Path.GetFileName(filePath);
        }

        public List<UdpPackage> GetSliceList()
        {
            return _slicesList;
        }

        /// <summary>
        /// 初始化包管理器
        /// </summary>
        /// <param name="headerSize">数据头长度</param>
        /// <param name="blockSize">块大小: 默认5M</param>
        /// <param name="sliceSize"></param>
        public UdpPackageManager(ushort headerSize, int blockSize = 5 * 1024 * 1024, ushort sliceSize = 1024)
        {
            HeaderSize = headerSize;
            BlockSize = blockSize > MaxBlockSize ? MaxBlockSize : blockSize;
            SliceSize = (sliceSize + HeaderSize) > MaxSliceSize ? MaxSliceSize : sliceSize;
        }
        /// <summary>
        /// 将文件读块
        /// </summary>
        /// <param name="filePath">文件的路径</param>
        /// <returns></returns>
        public async Task<byte[]> ReadFileToBlock(string filePath)
        {
            if (string.IsNullOrEmpty(FileName))
            {
                FileName = Path.GetFileName(filePath);
            }


            byte[] data;

            await using (FileStream fileRead = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                long temp = fileRead.Length - BlockSize * BlockSerial;
                if (temp <= 0)
                {
                    return null;
                }
                data = new byte[temp < BlockSize ? temp : BlockSize];
                fileRead.Seek(BlockSerial * (long)BlockSize, SeekOrigin.Begin);
                fileRead.Read(data, 0, (int)(temp < BlockSize ? temp : BlockSize));
                fileRead.Close();
                await fileRead.DisposeAsync();
            }

            return data;
        }
        /// <summary>
        /// 将块切割成片
        /// </summary>
        /// <param name="buffer">块数据</param>
        /// <param name="identity">文件标识</param>
        /// <returns></returns>
        public async Task<IQueueManager> BlockToSlice(byte[] buffer, byte identity)
        {
            IQueueManager queue = new QueueManager();
            ushort sliceCount = (ushort)(buffer.Length / SliceSize);
            if (buffer.Length % SliceSize > 0)
            {
                sliceCount++;
            }

            for (ushort i = 1; i <= sliceCount; i++)
            {
                var tmp = buffer.Length - (i - 1) * SliceSize;
                var blockSlicePackage = new UdpPackage()
                {
                    FileIdentity = identity,
                    OpCode = UdpOpCode.Message,
                    BlockSerial = BlockSerial,
                    SliceSerial = i,
                    SliceCount = sliceCount,
                    Buffer = buffer.Skip((i - 1) * SliceSize)
                        .Take(tmp < SliceSize ? tmp : SliceSize).ToArray()
                };

                await queue.EnqueueAsync(blockSlicePackage.PackageToBytes());
            }

            NextBlock();

            return queue;
        }

        /// <summary>
        ///  发送第一块
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public byte[] StartBlock(byte identity)
        {
            return new UdpPackage()
            {
                FileIdentity = identity,
                OpCode = UdpOpCode.Start,
                Buffer = Encoding.UTF8.GetBytes(FileName)
            }.PackageToBytes();
        }
        /// <summary>
        /// 发送最后一块
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public byte[] EndBlock(byte identity)
        {
            return new UdpPackage()
            {
                FileIdentity = identity,
                OpCode = UdpOpCode.End,
                Buffer = Encoding.UTF8.GetBytes(BlockSerial.ToString())
            }.PackageToBytes();
        }
        /// <summary>
        /// 收集接收端收到的切片
        /// </summary>
        /// <param name="buffer">数据片</param>
        /// <param name="lostSlices"></param>
        /// <param name="transferFinished"></param>
        /// <returns></returns>
        public bool CollectSlices(byte[] buffer, out ushort[] lostSlices, out bool transferFinished)
        {
            var package = (UdpPackage)new UdpPackage().BytesToUdpPackage(buffer);
            lostSlices = null;
            transferFinished = false;
            switch (package.OpCode)
            {
                case UdpOpCode.Start:
                    FileName = Encoding.UTF8.GetString(package.Buffer);
                    return false;
                case UdpOpCode.End:
                    return BlockIntegrityChecks(package.BlockSerial, out lostSlices, out transferFinished);
                case UdpOpCode.Message:
                    _slicesList.Add(package);
                    _slicesList = _slicesList.GroupBy(x => x.BlockSerial).Select(y => y.First()).ToList();

                    if (package.SliceCount
                        == _slicesList.Count(x => x.BlockSerial == BlockSerial))
                    {
                        return true;
                    }
                    return false;
            }
            return false;
        }
        /// <summary>
        /// 块检验
        /// </summary>
        /// <param name="blockSerial"></param>
        /// <param name="lostSlices"></param>
        /// <param name="transferFinished"></param>
        /// <returns></returns>
        public bool BlockIntegrityChecks(ushort blockSerial, out ushort[] lostSlices, out bool transferFinished)
        {
            if (blockSerial == BlockSerial)
            {
                lostSlices = null;
                transferFinished = false;
                return true;
            }

            var res = _slicesList.Where(x => x.BlockSerial == blockSerial)
                .Where(x => Enumerable.Range(0, blockSerial).Contains(x.BlockSerial)).ToList();
            var temp = res.Select(x => x.BlockSerial).ToList();
            temp.Insert(BlockSerial, 0);
            lostSlices = temp.ToArray();
            transferFinished = true;
            return false;
        }
        /// <summary>
        /// 将切片组装成块
        /// </summary>
        /// <param name="slices">切片</param>
        /// <param name="identity">文件标识</param>
        /// <returns></returns>
        public async Task<byte[]> SliceToBlock(List<UdpPackage> slices, byte identity)
        {
            var result =
                from package in slices
                where package.FileIdentity == identity && package.BlockSerial == BlockSerial
                orderby package.SliceSerial
                select package;
            if (result.Count() == 0)
            {
                // 重传块  BlockSerial就是缺少的块
            }
            var size = result.Sum(x => x.Buffer.Length);

            byte[] buffer = null;
            // ReSharper disable once PossibleNullReferenceException
            var thisBlock = slices.Where(x => x.BlockSerial == BlockSerial).ToList();
            if (thisBlock.Count == 0)
            {
                BlockSerial++;
                return null;
            }
            var packageSize = thisBlock[0].SliceCount;
            if (result.Count() == packageSize)
            {
                buffer = new byte[size];
                foreach (var buffPackage in result)
                {
                    buffPackage.Buffer.CopyTo(buffer, buffPackage.SliceSerial * SliceSize);
                }
                ClearOneBlockSlices(identity, BlockSerial);
                NextBlock();
            }
            else
            {
                // 重传切片
                var res = _slicesList.Where(x => x.BlockSerial == BlockSerial)
                    .Where(x => Enumerable.Range(0, packageSize).Contains(x.SliceSerial)).ToList();
                var temp = res.Select(x => x.SliceSerial).ToList();
                temp.Insert(BlockSerial, 0);
                // temp:    丢失的切片
            }
            return buffer;
        }
        /// <summary>
        /// 将数据块写入到文件中
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public async Task<bool> WriteBlockToFile(string filePath, byte[] buffer)
        {
            string path = $"{filePath}\\{FileName}";
            try
            {
                await using (FileStream fileSave1 = new FileStream(path, FileMode.Append,
                    FileAccess.Write))
                {
                    fileSave1.Write(buffer, 0, buffer.Length);
                    fileSave1.Flush();
                    fileSave1.Close();
                    await fileSave1.DisposeAsync();
                    return true;
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        /// <summary>
        /// 下一块数据
        /// </summary>
        public void NextBlock()
        {
            BlockSerial++;
        }
        /// <summary>
        /// 清除已经写入完成的数据
        /// </summary>
        /// <param name="identity">文件标识</param>
        /// <param name="blockSerial">数据块序号</param>
        public void ClearOneBlockSlices(byte identity, ushort blockSerial)
        {
            _slicesList =
                _slicesList.Where(x => (x.FileIdentity != identity) && (x.BlockSerial != blockSerial)).ToList();
        }
    }
}