using LungCare.SupportPlatform.UI.Windows.Common;
using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class ImageWindow : Window
    {
        public ImageWindow()
        {
            InitializeComponent();
        }

        BitmapSource BitmapSourceFromBase64(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            using (var stream = new MemoryStream(Convert.FromBase64String(value)))
            {
                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                BitmapSource result = decoder.Frames[0];
                result.Freeze();
                return result;
            }
        }


        public ImageWindow(BitmapSource bitmap)
        {
            InitializeComponent();
            borderMessage.MouseDown+=borderMessage_MouseDown;
            image.Source = bitmap;
        }

        public ImageWindow(string content)
        {
            InitializeComponent();

            image.Source = BitmapSourceFromBase64(content);
        }


        public string MsgText { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
