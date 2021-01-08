using System;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Chat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SuperSocket;
using SuperSocket.Channel;
using SuperSocket.Client;
using SuperSocket.ProtoBase;
using SuperSocket.Tests;
using Xunit;


namespace Chat.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Thread Receive = null;
        private IEasyClient<TextPackageInfo> _client = null;
        public string RemoteName = "All";

        public MainWindow()
        {
            InitializeComponent();
            TbUserName.Text = Dns.GetHostName();
        }

        private async Task InitTcp()
        {
            var options = new ChannelOptions
            {
                Logger = NullLogger.Instance,
                ReadAsDemand = true
            };

            _client = new EasyClient<TextPackageInfo>(new LinePipelineFilter(), options).AsClient();

            var connected = await _client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 4041));

            var connectPackage = new MessagePackage<TextMessageModel>()
            {
                OpCode = OpCode.Connect,
                MessageType = MessageType.TextMessage,
                Message = new TextMessageModel()
                {
                    LocalName = TbUserName.Text,
                    RemoteName = "Server"
                }
            };
            await _client.SendAsync(
                new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(connectPackage.ToString())));

            while (true)
            {
                var receivePackage =
                    JsonConvert.DeserializeObject<MessagePackage<TextMessageModel>>((await _client.ReceiveAsync()).Text);

                if (receivePackage.Message == null)
                {
                    continue;
                }
                switch (receivePackage.OpCode)
                {
                    case OpCode.DisConnect:
                        MessageBox.Show(receivePackage.Message.TextMessage, receivePackage.Message.LocalName);
                        TbUserName.IsEnabled = true;
                        break;
                    case OpCode.Connect:
                        LvOnlineList.Children.Clear();
                        foreach (var userClient in receivePackage.Clients)
                        {
                            LvOnlineList.Children.Add(new UserItemsControl(userClient.Username));
                        }

                        TbUserName.IsEnabled = false;
                        break;
                    case OpCode.All:
                        this.LbCurrentChat.Content = receivePackage.Message.LocalName;
                        WpChatArea.Children.Add(new MessageControl(new TextMessageModel()
                        {
                            LocalName = receivePackage.Message.LocalName,
                            TextMessage = receivePackage.Message.TextMessage
                        }));
                        break;
                    case OpCode.Single:
                        LbCurrentChat.Content = receivePackage.Message.RemoteName;
                        WpChatArea.Children.Add(new MessageControl(new TextMessageModel()
                        {
                            LocalName = receivePackage.Message.LocalName,
                            TextMessage = receivePackage.Message.TextMessage
                        }));
                        break;
                }

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

            //while (true)
            //{
            //    var package = await _client.ReceiveAsync();
            //    //if(package != null) SpChatArea.Text += "Receive:" + package.Text + Environment.NewLine;
            //    var data = JsonConvert.DeserializeObject<TextMessageModel>(package.Text);
            //    if (package != null)
            //    {
            //        WpChatArea.Children.Add(new MessageControl(new TextMessageModel()
            //        {
            //            LocalName = data.LocalName,
            //            TextMessage = data.TextMessage
            //        }));
            //    }
            //    await Task.Delay(500);
            //}
        }
        private static Random _rd = new Random();
        private void CreateUdpClient()
        {
            Socket udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            while (true)
            {
                EndPoint serverPoint = new IPEndPoint(IPAddress.Parse("192.168.31.215"), 4040);
                string message = "Console.ReadLine();";
                byte[] data = Encoding.UTF8.GetBytes(message);
                udpClient.SendTo(data, serverPoint);
                Thread.Sleep(3000);
            }
        }

        private async Task Send(string message)
        {
            ////await _client.SendAsync(Encoding.UTF8.GetBytes(message + "\r\n"));
            //var data = new TextMessageModel
            //{
            //    LocalName = TbUserName.Text,
            //    TextMessage = message
            //};
            //// 向ChatArea中添加组件
            //WpChatArea.Children.Add(new SendControl(data));
            //CreateUdpClient();

            ////await _client.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data) + "\r\n"));

            var sendPackage = new MessagePackage<TextMessageModel>()
            {
                OpCode = LbCurrentChat.Content.Equals("All") ? OpCode.All : OpCode.Single,
                MessageType = MessageType.TextMessage,
                Message = new TextMessageModel()
                {
                    LocalName = TbUserName.Text,
                    RemoteName = LbCurrentChat.Content.ToString(),
                    TextMessage = TbSendArea.Text
                }
            };
            WpChatArea.Children.Add(new SendControl(new TextMessageModel()
            {
                LocalName = TbUserName.Text,
                RemoteName = LbCurrentChat.Content.ToString(),
                TextMessage = TbSendArea.Text
            }));

            await _client.SendAsync(Encoding.UTF8.GetBytes(sendPackage.ToString()));
        }

        private void BtnSend_OnClick(object sender, RoutedEventArgs e)
        {
#pragma warning disable 4014
            Send(TbSendArea.Text);
#pragma warning restore 4014
            TbSendArea.Clear();
            TbSendArea.Focus();
        }

        private void BtnConnectServer_OnClick(object sender, RoutedEventArgs e)
        {
#pragma warning disable 4014
            InitTcp();
            TbUserName.IsEnabled = false;
#pragma warning restore 4014
        }
    }
}
