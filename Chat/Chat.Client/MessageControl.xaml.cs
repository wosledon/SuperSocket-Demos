using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Chat.Models;

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
            LabChatContext.Content = message.TextMessage;
        }

        public static System.Drawing.Image BytesToImage(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
            return image;
        }
    }
}
