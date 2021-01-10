using System;
using System.Collections.Generic;
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

namespace UdpClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static System.Net.Sockets.UdpClient _client = null;
        private static System.Net.Sockets.UdpClient _receiveClient = null;

        private Thread _receiveThread = null; 
        

        public MainWindow()
        {
            InitializeComponent();

            InitClient();
            InitThreads();
        }

        private void InitClient()
        {
            _client = new System.Net.Sockets.UdpClient(4040);
            _client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000));

            _receiveClient = new System.Net.Sockets.UdpClient(11000);
        }

        private void InitThreads()
        {
            _receiveThread = new Thread(new ThreadStart(ReceiveText));
            _receiveThread.Start();
        }

        private void DesThread()
        {
            _receiveThread.Abort();
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

        private async Task SendText(string Message)
        {
            var buffer = Encoding.ASCII.GetBytes(TbSendContext.Text);
            await _client.SendAsync(buffer, buffer.Length);
        }

        private void ReceiveText()
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                
                try
                {
                    var buffer = _receiveClient.Receive(ref remoteEndPoint);
                    if (buffer.Length > 0)
                    {
                        string message = Encoding.ASCII.GetString(buffer);
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

            var buffer = UdpHelper.ImageToBytes(fileName);
            ImageSend.Source = UdpHelper.BytesToBitmapImage(buffer);

            var packages = new UdpHelper(60000).PackageSlice(buffer, 1, UdpFileModel.Image);
            foreach (var package in packages)
            {
                await _client.SendAsync(package, package.Length);
                await Task.Delay(5);
            }
        }

        private void ReceiveImage()
        {
            
        }
    }
}
