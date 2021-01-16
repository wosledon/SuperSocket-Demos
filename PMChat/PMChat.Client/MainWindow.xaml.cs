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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualBasic.CompilerServices;
using PMChat.Models;
using SuperSocket.Channel;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace PMChat.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IEasyClient<TextPackageInfo> _sendTcpClient;
        private IEasyClient<TextPackageInfo> _sendImageTcpClient;

        private int _sendUdpPort = 11000;

        private bool _connected;

        private object _locker = new object();

        //private UdpClientManager _receiveClientManager;
        //private int _receiveUdpPort = 12000;

        public MainWindow()
        {
            InitializeComponent();

            //var _taskbar = (TaskbarIcon)FindResource("Taskbar");
            //_taskbar.ShowBalloonTip("标题", "内容", BalloonIcon.Info);
        }

        public void SetRoteName(string name)
        {
            LbChatWith.Content = name;
        }

        private void BtnConnectServer_OnClick(object sender, RoutedEventArgs e)
        {
            InitTcpConnectAndReceive();
            TbUserName.IsEnabled = false;
            BtnConnectServer.IsEnabled = false;
        }

        private async void InitTcpConnectAndReceive()
        {
            var options = new ChannelOptions
            {
                Logger = NullLogger.Instance,
                ReadAsDemand = true
            };

            _sendTcpClient = new EasyClient<TextPackageInfo>(new LinePipelineFilter(), options).AsClient();

            _connected = await _sendTcpClient.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8888));

            var connectPackage = new TcpPackage()
            {
                OpCode = OpCode.Connect,
                LocalName = TbUserName.Text,
                RemoteName = "Server",
                MessageType = MessageType.Text
            };

            await _sendTcpClient.SendAsync(
                new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(connectPackage.ToString())));


            while (true)
            {
                var receivePackage = TcpPackage.JsonToPackage((await _sendTcpClient.ReceiveAsync()).Text);
                if (string.IsNullOrEmpty(receivePackage.Message))
                {
                    continue;
                }

                switch (receivePackage.OpCode)
                {
                    case OpCode.Connect:
                        SpOnlineList.Children.Clear();
                        var allItem = new UserItemsControl("All");
                        allItem.setRoteName = SetRoteName;
                        SpOnlineList.Children.Add(allItem);
                        foreach (var onlineClient in receivePackage.Clients)
                        {
                            var childItem = new UserItemsControl(onlineClient.Username);
                            childItem.setRoteName = SetRoteName;
                            SpOnlineList.Children.Add(childItem);
                        }
                        TbUserName.IsEnabled = false;
                        BtnConnectServer.IsEnabled = false;
                        break;
                    case OpCode.DisConnect:
                        MessageBox.Show(receivePackage.Message, receivePackage.LocalName);
                        TbUserName.IsEnabled = true;
                        break;
                    case OpCode.All:
                    case OpCode.Single:
                        LbChatWith.Content = receivePackage.OpCode == OpCode.All
                            ? receivePackage.RemoteName : receivePackage.LocalName;
                        switch (receivePackage.MessageType)
                        {
                            case MessageType.Text:
                                ChatArea.Children.Add(new ReceiveControl(receivePackage, null));
                                break;
                            case MessageType.Image:
                                // 建立UDP客户端 直接接收消息
                                ReceiveImage(receivePackage: receivePackage);
                                break;
                            case MessageType.File:
                                var res = MessageBox.Show("是否接收文件?", "提示", MessageBoxButton.YesNo,
                                    MessageBoxImage.Information);
                                if (res == MessageBoxResult.Yes)
                                {
                                    // 发送TCP确认连接消息
                                    // 创建UDP客户端
                                }
                                break;
                        }
                        break;
                    case OpCode.Confirm:
                        switch (receivePackage.MessageType)
                        {
                            case MessageType.Image:
                                var fileName = await SendImage();
                                await Task.Delay(1000);
                                await SendImage(fileName, receivePackage);
                                break;
                            case MessageType.File:
                                break;
                        }
                        break;
                }

                Scr.ScrollToEnd();

                if (_connected)
                {
                    BdConnectState.Background = new SolidColorBrush(Colors.LimeGreen);
                    TbUserName.IsEnabled = false;
                }
                else
                {
                    BdConnectState.Background = new SolidColorBrush(Colors.OrangeRed);
                    TbUserName.IsEnabled = true;
                    break;
                }

                //await Task.Delay(1000);
            }
        }

        private async Task SendMessage(string message)
        {
            var sendPackage = new TcpPackage()
            {
                OpCode = LbChatWith.Content.Equals("All") ? OpCode.All : OpCode.Single,
                MessageType = MessageType.Text,
                LocalName = TbUserName.Text,
                RemoteName = LbChatWith.Content.ToString(),
                Message = message
            };

            ChatArea.Children.Add(new SendControl(sendPackage, null));
            var msg = sendPackage.ToString();
            await _sendTcpClient.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(msg)));
            Scr.ScrollToEnd();
        }

        private void BtnSendFile_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnSendImage_OnClick(object sender, RoutedEventArgs e)
        {
#pragma warning disable 4014
            // 发送确认包
            SendImageConfirm();
#pragma warning restore 4014
        }

        private async Task SendImageConfirm()
        {
            var confirmPackage = new TcpPackage()
            {
                OpCode = OpCode.Confirm,
                MessageType = MessageType.Image,
                LocalName = TbUserName.Text,
                RemoteName = LbChatWith.Content.ToString()
            };

            var confirmData = confirmPackage.ToString();
            await _sendTcpClient.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(confirmData)));
        }

        private async Task<string> SendImage()
        {
            var options = new ChannelOptions
            {
                Logger = NullLogger.Instance,
                ReadAsDemand = true
            };

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Title = "选择文件";
            openFileDialog.Filter = "jpg|*.jpg|jpeg|*.jpeg";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "jpg";
            bool? result = openFileDialog.ShowDialog();
            if (result != true)
            {
                return null;
            }
            string fileName = openFileDialog.FileName;

            var buffer = ImageHelper.ImageToBytes(fileName);
            ChatArea.Children.Add(new SendControl(new TcpPackage()
            {
                LocalName = TbUserName.Text,
                RemoteName = LbChatWith.Content.ToString()
            }, ImageHelper.BytesToBitmapImage(buffer)));
            Scr.ScrollToEnd();

            #region 发送接收确认码, 再创建UDP进行通信, 比较麻烦, 预留, 重写
            // 发送确认码, 并接收确认
            

            //while (packages.Length() > 0)
            //{
            //    var data = await packages.DequeueAsync();
            //    await _sendUdpImageClient.SendAsync(data, data.Length);
            //}

            //await _sendImageTcpClient.CloseAsync();
            //await _sendUdpImageClient.UdpClientCloseAsync();
            #endregion

            return fileName;
        }

        async Task SendImage(string fileName, TcpPackage package)
        {
            using (var _sendUdpImageClient = new UdpClientManager())
            {
                // 创建Udp客户端
                //await _sendUdpImageClient.CreateUdpSendClientAsync(new IPEndPoint(IPAddress.Loopback, _sendUdpPort));
                await _sendUdpImageClient.CreateUdpSendClientAsync(new IPEndPoint(IPAddress.Loopback, new Random().Next(40000, 50000)));
                var imagePackage = new TcpPackage()
                {
                    OpCode = LbChatWith.Content.Equals("All") ? OpCode.All : OpCode.Single,
                    MessageType = MessageType.Image,
                    LocalName = TbUserName.Text,
                    RemoteName = LbChatWith.Content.ToString()
                };

                #region Tcp校验 丢弃
                //_sendImageTcpClient = new EasyClient<TextPackageInfo>(new LinePipelineFilter(), options).AsClient();
                //var connected = await _sendImageTcpClient.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8888));

                //var msg = imagePackage.ToString();

                //await _sendImageTcpClient.SendAsync(Encoding.UTF8.GetBytes(msg));

                //UdpConfigPackage package = null;
                //while (true)
                //{
                //    var data = await _sendImageTcpClient.ReceiveAsync();
                //    package = TcpPackage.JsonToPackage(data.Text).Config;

                //    if (package != null)
                //    {
                //        break;
                //    }

                //    await Task.Delay(100);
                //}
                #endregion

                var point = package.Config.ReceiveEndPoint.Split("|");
                if (!_sendUdpImageClient.Client.Connected)
                {
                    _sendUdpImageClient.Connect(IPAddress.Parse(point[0]), Convert.ToInt32(point[1]));
                }

                // 发送Image
                var packageManager = new UdpPackageManager(8);
                var first = packageManager.StartBlock(1);
                await _sendUdpImageClient.SendAsync(first, first.Length);
                while (true)
                {
                    var imageBuffer = await packageManager.ReadFileToBlock(fileName);
                    //var imageBuffer = packageManager.ImageToBytes(fileName);
                    if (imageBuffer == null)
                    {
                        break;
                    }

                    var packages = await packageManager.BlockToSlice(imageBuffer, 1);
                    while (packages.Length() > 0)
                    {
                        var pBuffer = await packages.DequeueAsync();
                        await _sendUdpImageClient.SendAsync(pBuffer, pBuffer.Length);
                        await Task.Delay(1);
                    }
                }

                var end = packageManager.EndBlock(1);
                await _sendUdpImageClient.SendAsync(end, end.Length);
            }
        }

        private async void ReceiveImage(TcpPackage receivePackage)
        {
            var udpConfigPackage = receivePackage.Config;
            var remote = udpConfigPackage.ReceiveEndPoint.Split("|");
            var remotePoint = new IPEndPoint(IPAddress.Parse(remote[0]), Convert.ToInt32(remote[1]));

            using (var udpClient = await new UdpClientManager().CreateUdpReceiveClientAsync(12000))
            {
                var udpPackageManager = new UdpPackageManager(8);

                while (true)
                {
                    var buffer = (await udpClient.ReceiveAsync()).Buffer;
                    //var buffer = udpClient.Receive(ref remotePoint);
                    ushort[] lostSlice;
                    bool transferFinished;
                    if (udpPackageManager.CollectSlices(buffer, out lostSlice, out transferFinished))
                    {
                        if (!transferFinished)
                        {
                            // 只接收了一块数据就进行了显示操作
                            // 图片大小限定: 10M
                            // 解决方案: 将图片缓存到本地, 再读取到程序中
                            // 缓存方案: 像文件一样按块写入本地, 最后再进行调用
                            // 缓存地址: 软件安装目录
                            var buff = udpPackageManager.SliceToBlock(udpPackageManager.GetSliceList(), 1).Result;
                            if (buff != null)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    var bitmap = ImageHelper.BytesToBitmapImage(buff);
                                    ChatArea.Children.Add(new ReceiveControl(receivePackage, bitmap));
                                });
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }

                        //Thread.Sleep(10);
                    }
                }
                //MessageBox.Show("Client回收");
            }
            //udpClient.Client.Close(5000);
        }

        private void BtnSendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TbSendArea.Text))
            {
#pragma warning disable 4014
                SendMessage(TbSendArea.Text);
#pragma warning restore 4014
                TbSendArea.Clear();
                TbSendArea.Focus();
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            
        }
    }
}
