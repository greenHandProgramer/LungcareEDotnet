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

namespace LungCare.SupportPlatform.UI
{
    /// <summary>
    /// Dicom成功上传.xaml 的交互逻辑
    /// </summary>
    public partial class Dicom成功上传 : Window
    {
        public EventHandler Evt继续上传 { get; set; }
        public EventHandler Evt完成 { get; set; }

        public Dicom成功上传()
        {
            InitializeComponent();
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            Close();
            if (Evt完成 != null)
            {
                Evt完成(this, e);
            }
        }

        private void btnUploadAnother_Click(object sender, RoutedEventArgs e)
        {
            Close();
            if (Evt继续上传 != null)
            {
                Evt继续上传(this, e);
            }
        }
    }
}
