using System.Windows.Controls;
using System.Windows.Input;

namespace Chat.Client
{
    /// <summary>
    /// UserItemsControl.xaml 的交互逻辑
    /// </summary>
    public partial class UserItemsControl : UserControl
    {
        public delegate void SetRoteName(string name);

        public SetRoteName setRoteName;

        public UserItemsControl(string username)
        {
            InitializeComponent();

            TbUserName.Text = username;
        }

        private void UserItemsControl_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            setRoteName(TbUserName.Text);
        }
    }
}
