using System;
using System.Collections.Generic;
using System.Linq;
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
using PMChat.Models;

namespace PMChat.Client
{
    /// <summary>
    /// SendControl.xaml 的交互逻辑
    /// </summary>
    public partial class SendControl : UserControl
    {
        public SendControl(TcpPackage package, BitmapImage image)
        {
            InitializeComponent();

            LbSayTo.Content = $"[ {package.RemoteName} ]";
            TbChatContext.Text = package.Message;

            if(image != null)
            {
                TbChatContext.Width = 100;
                TbChatContext.Height = 100;
                TbImage.ImageSource = image;
            }
            else
            {
                TbChatContext.Background = new SolidColorBrush(Colors.LightSeaGreen);
            }
        }
    }
}
