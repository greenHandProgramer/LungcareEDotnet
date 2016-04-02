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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LungCare.SupportPlatform.UI.UserControls
{
    /// <summary>
    /// UploadListUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class UploadListUserControl : UserControl
    {
        Models.DataListItem[] _datalist;

        public void StartLoading()
        {
            lbLoadingPrompt.Visibility = Visibility.Visible;
        }
        public void FinishLoading()
        {
            lbLoadingPrompt.Visibility = Visibility.Hidden;
        }

        public void SetDataSource(Models.DataListItem[] dataSource)
        {
            _datalist = dataSource;
            _datalist = _datalist.OrderBy(item => string.IsNullOrEmpty(item.UploadTimestamp)? DateTime.MinValue : DateTime.ParseExact(item.UploadTimestamp, "yyyyMMdd HHmmss", null)).Reverse().ToArray();

            string text2Search = tbSearch.Text.ToLower();

            listView.DataContext =
                new System.ComponentModel.BindingList<Models.DataListItem>(
                    new List<Models.DataListItem>(
                        _datalist.Where(item => item.Status != "待上传" && (item.PatientName.ToLower().Contains(text2Search)|| item.DataID.ToLower().Contains(text2Search)))));
            //listView.DataContext = new System.ComponentModel.BindingList<Models.DataListItem>(dataSource);
            listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
        }

        public UploadListUserControl()
        {
            InitializeComponent();
        }

        private void linkLabel_Click(object sender, RoutedEventArgs e)
        {
            ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
            string UploadTimestamp = LinkLabel.Tag.ToString();
            Console.WriteLine(LinkLabel.Tag.ToString());

            IEnumerable<Models.DataListItem> list = listView.Items.SourceCollection.Cast<Models.DataListItem>();
            var items = list.First(item => ((Models.DataListItem)item).UploadTimestamp == UploadTimestamp);

            UI.MsgWindow MsgWindow = new UI.MsgWindow();
            MsgWindow.MsgText = string.Format(@"患者姓名：{0} 年龄：{1}
20150920 09:10 上传CT
20150920 10:10 后台核对图像
.......


如果图像无法处理，这里显示拒绝原因！", items.PatientSex, items.PatientAge);
            MsgWindow.MsgText = items.AdditionalData;

            MsgWindow.AllowsTransparency = true;
            MsgWindow.Background = System.Windows.Media.Brushes.Transparent;
            MsgWindow.OpacityMask = System.Windows.Media.Brushes.White;
            MsgWindow.Owner = MainWindow.Instance;

            MsgWindow.ShowDialog();
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text2Search = tbSearch.Text.ToLower();

            if (_datalist == null || _datalist.Length == 0)
            {
                return;
            }
            listView.DataContext =
                new System.ComponentModel.BindingList<Models.DataListItem>(
                    new List<Models.DataListItem>(
                        _datalist.Where(item => item.Status != "待上传" && (item.PatientName.ToLower().Contains(text2Search)|| item.DataID.ToLower().Contains(text2Search)))));
            //listView.DataContext = new System.ComponentModel.BindingList<Models.DataListItem>(dataSource);
            listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
        }

        //private void ShowOrderDetail_Click(object sender, RoutedEventArgs e)
        //{
        //    ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
        //    string PatientName = LinkLabel.Tag.ToString();
        //    Console.WriteLine(LinkLabel.Tag.ToString());

        //    IEnumerable<LungCare.SupportPlatform.Models.CTDicomInfo> list =
        //        datagrid.Items.SourceCollection.Cast<LungCare.SupportPlatform.Models.CTDicomInfo>();

        //    var items = list.First(item => ((LungCare.SupportPlatform.Models.CTDicomInfo)item).PatientName == PatientName);

        //    LungCare.SupportPlatform.UI.MsgWindow MsgWindow = new LungCare.SupportPlatform.UI.MsgWindow();
        //    MsgWindow.MsgText = items.AdditionalData;

        //    MsgWindow.AllowsTransparency = true;
        //    MsgWindow.Background = System.Windows.Media.Brushes.Transparent;
        //    MsgWindow.OpacityMask = System.Windows.Media.Brushes.White;

        //    MsgWindow.ShowDialog();
        //}
    }

   
}
