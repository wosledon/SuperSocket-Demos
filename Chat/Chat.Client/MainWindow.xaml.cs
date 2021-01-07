using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Chat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SuperSocket.Channel;
using SuperSocket.Client;
using SuperSocket.ProtoBase;
using Xunit;


namespace Chat.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread _receive = null;
        private IEasyClient<TextPackageInfo> _client = null;
        public MainWindow()
        {
            InitializeComponent();
#pragma warning disable 4014
            Init();
#pragma warning restore 4014
        }

        private async Task Init()
        {
            var options = new ChannelOptions
            {
                Logger = NullLogger.Instance,
                ReadAsDemand = true
            };

            _client = new EasyClient<TextPackageInfo>(new LinePipelineFilter(), options).AsClient();

            var connected = await _client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 4041));
            Assert.True(connected);

            //for (var i = 0; i < 100; i++)
            //{
            //    var msg = Guid.NewGuid().ToString();
            //    await _client.SendAsync(Encoding.UTF8.GetBytes(msg + "\r\n"));
            //}

            while (true)
            {
                var package = await _client.ReceiveAsync();
                Assert.NotNull(package);
                //if(package != null) SpChatArea.Text += "Receive:" + package.Text + Environment.NewLine;
                var data = JsonConvert.DeserializeObject<TextMessageModel>(package.Text);
                if (package != null)
                {
                    SpChatArea.Children.Add(new MessageControl(new TextMessageModel()
                    {
                        Name = data.Name,
                        TextMessage = data.TextMessage
                    }));
                }
                await Task.Delay(500);
            }
        }

        private async Task Send(string message)
        {
            //await _client.SendAsync(Encoding.UTF8.GetBytes(message + "\r\n"));
            var data = new TextMessageModel
            {
                Name = TbUserName.Text,
                TextMessage = message
            };
            // 向ChatArea中添加组件
            SpChatArea.Children.Add(new SendControl(data));

            await _client.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data) + "\r\n"));
        }

        private void BtnSend_OnClick(object sender, RoutedEventArgs e)
        {
#pragma warning disable 4014
            Send(TbSendArea.Text);
#pragma warning restore 4014
        }
    }
}
