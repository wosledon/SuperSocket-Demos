using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
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
using System.Net.Sockets;
using System.Threading;
using Microsoft.Win32;

namespace UdpClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static System.Net.Sockets.UdpClient _client = null;
        private static System.Net.Sockets.UdpClient _receiveClient = null;

        private static Object locker = new object();

        private Thread _receiveThread = null;

        private static int _receiveThreadNum = 20;
        private Thread[] _receiveThreads = new Thread[_receiveThreadNum];

        private Random rand = new Random();

        private int threadDone = 0;

        UdpHelper _udpHelper = new UdpHelper();

        private System.Net.Sockets.UdpClient receivingUdpClient = null;


        public MainWindow()
        {
            InitializeComponent();

            InitClient();
            InitThreads();
        }

        private void InitClient()
        {
            _client = new System.Net.Sockets.UdpClient(11000);
            _client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12000));

            //_receiveClient = new System.Net.Sockets.UdpClient(11000);
            receivingUdpClient = new System.Net.Sockets.UdpClient(12000);
        }

        private void InitThreads()
        {
            //_receiveThread = new Thread(new ThreadStart(ReceiveText));
            //_receiveThread.Start();

            for (int i = 0; i < _receiveThreadNum; i++)
            {
                //_receiveThreads[i] = new Thread(new ThreadStart(ReceiveImage));
                _receiveThreads[i] = new Thread(new ThreadStart(ReceiveFile));
                _receiveThreads[i].Start();
            }
        }

        private void DesThread()
        {
            //_receiveThread.Abort();

            foreach (var thread in _receiveThreads)
            {
                thread.Abort();
            }
        }

        private void BtnSendText_OnClick(object sender, RoutedEventArgs e)
        {
#pragma warning disable 4014
            SendText();
#pragma warning restore 4014
        }

        private async Task SendText()
        {
            try
            {
                var buffer = Encoding.ASCII.GetBytes(TbSendContext.Text);
                await _client.SendAsync(buffer, buffer.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task SendText(string message)
        {
            var buffer = Encoding.ASCII.GetBytes(TbSendContext.Text);
            await _client.SendAsync(buffer, buffer.Length);
        }

        private async Task ReceiveText()
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                
                try
                {
                    //var buffer = _receiveClient.Receive(ref remoteEndPoint);
                    var buffer = await _receiveClient.ReceiveAsync();
                    if (buffer.Buffer.Length > 0)
                    {
                        string message = Encoding.ASCII.GetString(buffer.Buffer);
                        Dispatcher.Invoke(() => { TbReceiveContext.Text += message + Environment.NewLine; });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private void BtnSendImage_OnClick(object sender, RoutedEventArgs e)
        {
#pragma warning disable 4014
            SendImage();
#pragma warning restore 4014
        }

        private async Task SendImage()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Title = "选择文件";
            openFileDialog.Filter = "jpg|*.jpg|jpeg|*.jpeg|*.txt";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "jpg";
            bool? result = openFileDialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            string fileName = openFileDialog.FileName;

            var buffer = UdpHelper.ImageToBytes(fileName);
            ImageSend.Source = UdpHelper.BytesToBitmapImage(buffer);

            var packages = new UdpHelper(65000).PackageSlice(buffer, 1, UdpFileModel.Image);
            var c = 0;
            
            foreach (var package in packages)
            {
                await _client.SendAsync(package, package.Length);
                LbSendImageProcess.Content = $"{++c}/{packages.Count}";
                await Task.Delay(1);
            }
        }
        int count = 0;
        private void ReceiveImage()
        {
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                lock (locker)
                {
                    try
                    {
                        Byte[] receiveBytes = receivingUdpClient.Receive(ref remoteIpEndPoint);
                        var op = _udpHelper.PackageConcat(receiveBytes);
                        if (op == 0)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                //LbReceive.Content = $"{++count + 1}";
                                LbReceiveImageProcess.Content = $"{++count}/{UdpHelper.Packages.Count}";
                            });
                        }
                        if (op == 1)
                        {
                            var img = _udpHelper.PackageMerge(UdpHelper.Packages, 1);
                            Dispatcher.Invoke(() =>
                            {
                                ImageReceive.Source = UdpHelper.BytesToBitmapImage(img);
                                count = 0;
                            });
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
            DesThread();
        }

        private void BtnSendFile_OnClick(object sender, RoutedEventArgs e)
        {
#pragma warning disable 4014
            SendFile();
#pragma warning restore 4014
        }

        private async Task SendFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "压缩文件|*.zip;*.jar;*.rar";//文件扩展名
            dialog.CheckFileExists = true;
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.FileName))//可以上传压缩包.zip 或者jar包
            {
                try
                {
                    byte[] buffer = UdpHelper.FileToBytes(dialog.FileName);//文件转成byte二进制数组

                    var packages = new UdpHelper(65000).PackageSlice(buffer, 1, UdpFileModel.File);
                    var c = 0;

                    foreach (var package in packages)
                    {
                        await _client.SendAsync(package, package.Length);
                        LbSendFileProcess.Content = $"{++c}/{packages.Count}";
                        await Task.Delay(1);
                    }
                    UdpHelper.ClearPackages(1);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        private void ReceiveFile()
        {
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            //var udpClient = new System.Net.Sockets.UdpClient();
            while (true)
            {
                Byte[] receiveBytes = receivingUdpClient.Receive(ref remoteIpEndPoint);
                //var name = Thread.CurrentThread.ManagedThreadId.ToString();
                try
                {
                    //Dispatcher.Invoke(() =>
                    //{
                    //    TbReceiveContext.Text += name + Environment.NewLine;
                    //    TbReceiveContext.ScrollToEnd();
                    //});
                    //Byte[] receiveBytes = udpClient.Receive(ref remoteIpEndPoint);
                    var op = _udpHelper.PackageConcat(receiveBytes);
                    if (op == 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            //LbReceive.Content = $"{++count + 1}";
                            LbReceiveFileProcess.Content = $"{++count}/{UdpHelper.Packages[0].PackageCount}";
                        });
                    }
                    if (op == 1)
                    {
                        var buffer = _udpHelper.PackageMerge(UdpHelper.Packages, 1);
                        Dispatcher.Invoke(() =>
                        {
                            //LbReceiveFileProcess.Content = $"{++count}/{UdpHelper.Packages[0].PackageCount}";
                            UdpHelper.BytesToFile(buffer, $"E://1.zip");
                            UdpHelper.ClearPackages(1);
                            MessageBox.Show("接收成功!");
                            count = 0;
                        });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
