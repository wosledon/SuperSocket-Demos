using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace UdpBlockTransferClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            InitClients();
            InitThreads();
        }

        private UdpClient _sendClient = null;
        private UdpClient _receiveClient = null;

        private readonly int _threadNums = 25;
        private Thread[] _threads = null;

        private UdpHelper _udpHelper = new UdpHelper();

        private Object locker = new object();

        private void InitClients()
        {
            _sendClient = new UdpClient(4040);
            _sendClient.Connect(IPAddress.Parse("127.0.0.1"), 11000);

            _receiveClient = new UdpClient(11000);
        }

        private void InitThreads()
        {
            _threads = new Thread[_threadNums];

            for (int i = 0; i < _threadNums; i++)
            {
                _threads[i] = new Thread(new ThreadStart(ReceiveFile));
                _threads[i].Start();
            }
        }

        private void DesThreads()
        {
            foreach (var thread in _threads)
            {
                thread.Abort();
            }
        }


        private void BtnSendFile_OnClick(object sender, RoutedEventArgs e)
        {
#pragma warning disable 4014
            SendFile();
#pragma warning restore 4014
        }

        private int sCount = 0;
        private async Task SendFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "压缩文件|*.zip;*.jar;*.rar;*.txt";//文件扩展名
            dialog.CheckFileExists = true;
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.FileName))//可以上传压缩包.zip 或者jar包
            {
                try
                {
                    UdpHelper sendHelper = new UdpHelper(dialog.FileName);
                    await _sendClient.SendAsync(sendHelper.StartBlock(1), sendHelper.StartBlock(1).Length);
                    while (true)
                    {
                        var buffer = await sendHelper.FileDicedBytes(dialog.FileName);
                        if (buffer == null)
                        {
                            break;
                        }

                        var packages = sendHelper.BlockSlice(buffer, 1);
                        foreach (var package in packages)
                        {
                            await _sendClient.SendAsync(package, package.Length);
                            Dispatcher.Invoke(() =>
                            {
                                LbSendProcess.Content = $"{++sCount}";
                            });
                            await Task.Delay(1);
                        }
                    }
                    await _sendClient.SendAsync(sendHelper.EndBlock(1), sendHelper.EndBlock(1).Length);
                    await Task.Delay(5);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private int rCount = 0;
        private void ReceiveFile()
        {
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            //var udpClient = new System.Net.Sockets.UdpClient();
            while (true)
            {
                Byte[] receiveBytes = _receiveClient.Receive(ref remoteIpEndPoint);
                //var name = Thread.CurrentThread.ManagedThreadId.ToString();
                Dispatcher.Invoke(() =>
                {
                    LbReceiveProcess.Content = $"{++rCount}";
                });
                lock (locker)
                {
                    try
                    {
                        ushort[] lostSlice = null;
                        if (_udpHelper.SlicePackageConcat(receiveBytes, out lostSlice))
                        {
                            var buffer = _udpHelper.SlicePackageMergeToBlock(_udpHelper.BlockSlicePackages, 1);
                            _udpHelper.AppendDataToFile("E:", buffer);
                            Dispatcher.Invoke(() =>
                            {
                                LbReceiveProcess.Content = $"{++rCount}";
                                MessageBox.Show("接收完成!");
                            });
                        }

                        if (lostSlice != null)
                        {
                            // 丢包重传
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            DesThreads();
        }
    }
}
