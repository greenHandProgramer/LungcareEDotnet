using LungCare.SupportPlatform.UI.Windows.Common;
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
using System.Windows.Shapes;

namespace LungCare.SupportPlatform.UI
{
    /// <summary>
    /// MsgWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MsgWindow : Window
    {
        public MsgWindow()
        {
            InitializeComponent();
        }

        public string MsgText { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            label.Content = MsgText;
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void borderMessage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.BringIntoView();
        }
    }
}
