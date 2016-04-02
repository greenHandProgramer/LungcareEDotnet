using System.Linq;
using System;
using System.Collections.Generic;
using System.Windows;
using ThemedControlsLibrary;
using System.ComponentModel;

namespace LungCare.SupportPlatform.UI
{
    /// <summary>
    /// 上传列表Wnd.xaml 的交互逻辑
    /// </summary>
    public partial class 上传列表Wnd : Window
    {
        public 上传列表Wnd()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void linkLabel_Click(object sender, RoutedEventArgs e)
        {
            LinkLabel LinkLabel = (LinkLabel)sender;
            string 单号 = LinkLabel.Tag.ToString();
            Console.WriteLine(LinkLabel.Tag.ToString());

            IEnumerable<上传列表Entity> list = listView.Items.SourceCollection.Cast<上传列表Entity>();
            var items = list.First(item => ((上传列表Entity)item).单号 == 单号);

            LungCare.SupportPlatform.UI.MsgWindow MsgWindow = new LungCare.SupportPlatform.UI.MsgWindow();
            MsgWindow.MsgText = string.Format(@"患者姓名：{0} 年龄：{1}
20150920 09:10 上传CT
20150920 10:10 后台核对图像
.......


如果图像无法处理，这里显示拒绝原因！", items.性别, items.年龄);

            MsgWindow.AllowsTransparency = true;
            MsgWindow.Background = System.Windows.Media.Brushes.Transparent;
            MsgWindow.OpacityMask = System.Windows.Media.Brushes.White;

            MsgWindow.ShowDialog();
        }

        private void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            //List<Models.CTDicomInfo> list = new List<Models.CTDicomInfo>();
            //list.Add(new Models.CTDicomInfo());

            List<上传列表Entity> list = new List<上传列表Entity>();
            list.Add(new 上传列表Entity());

            listView.DataContext = null;
            listView.DataContext = new BindingList<上传列表Entity>(list);
            listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
        }
    }

    public class 上传列表Entity
    {
        public string 时间 { get; set; }
        public string 单号 { get; set; }
        public string 患者姓名 { get; set; }
        public string 性别 { get; set; }
        public string 年龄 { get; set; }

        public string 状态 { get; set; }
    }
}
