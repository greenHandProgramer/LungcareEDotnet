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

namespace LungCare.SupportPlatform.UI.UserControls
{
    /// <summary>
    /// UploadListUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class FileItemUserControl1 : UserControl
    {
        Models.DataListItem[] _datalist;

        public void StartLoading()
        {
        }
        public void FinishLoading()
        {
        }

        public void SetDataSource(Models.DataListItem[] dataSource)
        {
            _datalist = dataSource;
            _datalist = _datalist.OrderBy(item => string.IsNullOrEmpty(item.UploadTimestamp)? DateTime.MinValue : DateTime.ParseExact(item.UploadTimestamp, "yyyyMMdd HHmmss", null)).Reverse().ToArray();


        }

        public FileItemUserControl1()
        {
            InitializeComponent();

            //borderFileItem.MouseLeftButtonDown += borderFileItem_MouseLeftButtonDown;
            //borderFileItem.MouseEnter += borderFileItem_MouseEnter;
            //borderFileItem.MouseLeave += borderFileItem_MouseLeave;

            //lbFileName.MouseLeftButtonDown += borderFileItem_MouseLeftButtonDown;
            //lbFileName.MouseEnter += borderFileItem_MouseEnter;
            //lbFileName.MouseLeave += borderFileItem_MouseLeave;
        }
        private void borderFileItem_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
           // borderFileItem.Opacity = 0.6;
        }
        private void borderFileItem_MouseLeave(object sender, RoutedEventArgs e)
        {
           // borderFileItem.Opacity = 0;
        }

        private void borderFileItem_MouseEnter(object sender, RoutedEventArgs e)
        {
           // borderFileItem.Opacity = 0.2;
        }
        private void linkLabel_Click(object sender, RoutedEventArgs e)
        {
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

    }

 
}
