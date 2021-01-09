using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace Chat.UdpDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            init();
        }

        private UdpClient udpClient;
        void init()
        {
            udpClient = new UdpClient(4042);
            udpClient.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000));
        }
        

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            //var client = getSocket();
            //client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));
            //client.Send(Encoding.ASCII.GetBytes("Test"));

            //UdpClient udpClietn = new UdpClient(4042);
            //udpClietn.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));

            var buffer = Encoding.ASCII.GetBytes(TextBox.Text);
            udpClient.Send(buffer, buffer.Length);

        }
        private static Random _rd = new Random();
        private Socket getSocket()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var localPort = _rd.Next(40000, 50000);
            var localEndPoint = new IPEndPoint(IPAddress.Loopback, localPort);
            socket.Bind(localEndPoint);
            return socket;
        }

        private void BtnReceive_OnClick(object sender, RoutedEventArgs e)
        {
            Thread recThreead = new Thread(() => Receive());
            recThreead.Start();
        }

        async Task Receive()
        {
            UdpClient receivingUdpClient = new UdpClient(11000);

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                
                try
                {
                    Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    Dispatcher.Invoke(() =>
                    {
                        TbReceive.Text += returnData + Environment.NewLine;
                    });

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        async Task ImgReceive(UdpTools udpTools)
        {
            UdpClient receivingUdpClient = new UdpClient(11000);

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            int count = 0;

            while (true)
            {

                try
                {
                    Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    var op = udpTools.PackageConcat(receiveBytes);
                    if (op == 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LbReceive.Content = $"{++count + 1}";
                        });
                    }
                    if (op == 1)
                    {
                        var img = udpTools.PackageMerge(UdpTools.Packages, 1);
                        Dispatcher.Invoke(() =>
                        {
                            ImageReceive.Source = UdpTools.BytesToBitmapImage(img);
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

        private void BtnImageSend_OnClick(object sender, RoutedEventArgs e)
        {
            

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

            var buffer = UdpTools.ImageToBytes(fileName);
            ImageSend.Source = UdpTools.BytesToBitmapImage(buffer);

            send(buffer);
        }

        private async Task send(byte[] buffer)
        {
            var udpTools = new UdpTools();
            var packages = udpTools.PackageSlice(buffer, 1);
            var count = 0;
            foreach (var package in packages)
            {
                udpClient.Send(package, package.Length);
                LbSend.Content = $"{++count}/{packages.Count}";
                await Task.Delay(5);
            }
        }

        private void BtnImageReceive_OnClick(object sender, RoutedEventArgs e)
        {
            var udpTools = new UdpTools();
#pragma warning disable 4014
            Thread udp = new Thread(() => ImgReceive(udpTools));
#pragma warning restore 4014
            udp.Start();
        }

        private void BtnImageOpen_OnClick(object sender, RoutedEventArgs e)
        {
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
            //BitmapImage image = new BitmapImage(new Uri(fileName));
            //ImageSend.Source = image;

            var buffer = UdpTools.ImageToBytes(fileName);
            ImageSend.Source = UdpTools.BytesToBitmapImage(buffer);
            //var a = UdpTools.BitmapImageToByteArray(image);
            //ImageSend.Source = UdpTools.ByteArrayToBitmapImage(a);
        }
    }
}
