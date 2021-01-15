using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        private UdpClientManager _sendUdpImageClient;
        private int _sendUdpPort = 11000;

        private UdpClientManager _receiveClientManager;
        private int _receiveUdpPort = 12000;



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
#pragma warning disable 4014
            InitTcpConnectAndReceive();
#pragma warning restore 4014
            TbUserName.IsEnabled = false;
            BtnConnectServer.IsEnabled = false;
        }

        private async Task InitTcpConnectAndReceive()
        {
            var options = new ChannelOptions
            {
                Logger = NullLogger.Instance,
                ReadAsDemand = true
            };

            _sendTcpClient = new EasyClient<TextPackageInfo>(new LinePipelineFilter(), options).AsClient();

            var connected = await _sendTcpClient.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8888));

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
                                await ReceiveImage(receivePackage: receivePackage);
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
                }

                Scr.ScrollToEnd();

                if (connected)
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

                await Task.Delay(500);
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
            await _sendTcpClient.SendAsync(Encoding.UTF8.GetBytes(msg));
            Scr.ScrollToEnd();
        }

        private void BtnSendFile_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnSendImage_OnClick(object sender, RoutedEventArgs e)
        {
#pragma warning disable 4014
            SendImage();
#pragma warning restore 4014
        }

        private async Task SendImage()
        {
            var options = new ChannelOptions
            {
                Logger = NullLogger.Instance,
                ReadAsDemand = true
            };

            _sendImageTcpClient = new EasyClient<TextPackageInfo>(new LinePipelineFilter(), options).AsClient();
            var connected = await _sendImageTcpClient.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8888));

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
                return;
            }
            string fileName = openFileDialog.FileName;

            var buffer = ImageHelper.ImageToBytes(fileName);
            ChatArea.Children.Add(new SendControl(new TcpPackage()
            {
                LocalName = TbUserName.Text
            }, ImageHelper.BytesToBitmapImage(buffer)));

            var imagePackage = new TcpPackage()
            {
                OpCode = LbChatWith.Content.Equals("All") ? OpCode.All : OpCode.Single,
                MessageType = MessageType.Image,
                LocalName = TbUserName.Text,
                RemoteName = LbChatWith.Content.ToString()
            };

            var msg = imagePackage.ToString();

            await _sendImageTcpClient.SendAsync(Encoding.UTF8.GetBytes(msg));

            Scr.ScrollToEnd();

            UdpConfigPackage package = null;
            while (true)
            {
                var data = await _sendImageTcpClient.ReceiveAsync();
                package = TcpPackage.JsonToPackage(data.Text).Config;

                if (package != null)
                {
                    break;
                }

                await Task.Delay(100);
            }

            _sendUdpImageClient = new UdpClientManager();
            await _sendUdpImageClient.CreateUdpSendClientAsync(new IPEndPoint(IPAddress.Loopback, _sendUdpPort));
            var point = package.ReceiveEndPoint.Split("|");
            _sendUdpImageClient.Connect(IPAddress.Parse(point[0]), Convert.ToInt32(point[1]));

            var packageManager = new UdpPackageManager(8, fileName);
            var first = packageManager.StartBlock(1);
            await _sendUdpImageClient.SendAsync(first, first.Length);
            while (true)
            {
                var imageBuffer = await packageManager.ReadFileToBlock(fileName);
                if (imageBuffer == null)
                {
                    break;
                }

                var packages = await packageManager.BlockToSlice(imageBuffer, 1);
                while (packages.Length() > 0)
                {
                    var pBuffer = await packages.DequeueAsync();
                    await _sendUdpImageClient.SendAsync(pBuffer, pBuffer.Length);
                }
            }
            await _sendUdpImageClient.SendAsync(packageManager.EndBlock(1), packageManager.EndBlock(1).Length);

            //while (packages.Length() > 0)
            //{
            //    var data = await packages.DequeueAsync();
            //    await _sendUdpImageClient.SendAsync(data, data.Length);
            //}

            await _sendImageTcpClient.CloseAsync();
            //await _sendUdpImageClient.UdpClientCloseAsync();
        }

        private async Task ReceiveImage(TcpPackage receivePackage)
        {
            var udpConfigPackage = receivePackage.Config;
            var remote = udpConfigPackage.ReceiveEndPoint.Split("|");
            var remotePoint = new IPEndPoint(IPAddress.Parse(remote[0]), Convert.ToInt32(remote[1]));
            var udpClient = await new UdpClientManager().CreateUdpReceiveClientAsync(12000);
            
            var udpPackageManager = new UdpPackageManager(8);

            while (true)
            {
                var buffer = (await udpClient.ReceiveAsync()).Buffer;
                ushort[] lostSlice;
                bool transferFinished;
                if (udpPackageManager.CollectSlices(buffer, out lostSlice, out transferFinished))
                {
                    if (!transferFinished)
                    {
                        var buff = await udpPackageManager.SliceToBlock(udpPackageManager.GetSliceList(), 1);
                        if (buff != null)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                ChatArea.Children.Add(new ReceiveControl(receivePackage,
                                    ImageHelper.BytesToBitmapImage(buff)));
                            });
                        }
                    }
                    else
                    {
                        break;
                    }

                    Thread.Sleep(10);
                }
            }
        }

        private void BtnSendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TbSendArea.Text))
            {
#pragma warning disable 4014
                SendMessage(TbSendArea.Text);
#pragma warning restore 4014
            }
        }
    }
}
