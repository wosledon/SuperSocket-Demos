using Chat.Models;
using System.Windows.Controls;

namespace Chat.Client
{
    /// <summary>
    /// MessageControl.xaml 的交互逻辑
    /// </summary>
    public partial class MessageControl : UserControl
    {

        public MessageControl(TextMessageModel message)
        {
            InitializeComponent();

            LabChatName.Content = $"[ {message.LocalName} ]";
            //LabChatContext.Content = message.TextMessage;
            TbChatContext.Text = message.TextMessage;
        }

        //public static System.Drawing.Image BytesToImage(byte[] buffer)
        //{
        //    MemoryStream ms = new MemoryStream(buffer);
        //    System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
        //    return image;
        //}
    }
}
