using Chat.Models;
using System.Windows.Controls;

namespace Chat.Client
{
    /// <summary>
    /// ReceiveControl.xaml 的交互逻辑
    /// </summary>
    public partial class SendControl : UserControl
    {
        public SendControl(TextMessageModel message)
        {
            InitializeComponent();

            //LabChatName.Content = $"[ {message.LocalName} ]";
            TbChatContext.Text = message.TextMessage;
        }
    }
}
