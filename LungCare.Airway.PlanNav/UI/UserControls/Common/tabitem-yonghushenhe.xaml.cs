using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using LungCare.SupportPlatform.Models;

namespace LungCare.SupportPlatform.UI.UserControls.Common
{
	/// <summary>
	/// tabitem_yonghushenhe.xaml 的交互逻辑
	/// </summary>
	public partial class tabitem_yonghushenhe : UserControl
	{
		public tabitem_yonghushenhe()
		{
			this.InitializeComponent();
		}

        List<GetUserInfoResponse> _Allapprove = new List<GetUserInfoResponse>();
        private int _approveMaxPage = 0;
        private int _approveCurrentPage = 0;
        private int _approvePageSize = 13;
        public event EventHandler<RoutedEventArgs> UpdateHandler;


        public void SetDataSource(List<GetUserInfoResponse> dataSource)
        {
            _Allapprove = dataSource;
            _approveCurrentPage = 0;
            if (_Allapprove.Count() % _approvePageSize == 0)
            {
                _approveMaxPage = _Allapprove.Count() / _approvePageSize - 1;
            }
            else
            {
                _approveMaxPage = _Allapprove.Count() / _approvePageSize;
            }
            checkPage();
            LoadPage(_approveCurrentPage, _approvePageSize, _Allapprove);
        }

        private void detailLinkLabel_Click(object sender, RoutedEventArgs e)
        {
            ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
            string phoneNumber = LinkLabel.Tag.ToString();
            Console.WriteLine(LinkLabel.Tag.ToString());

            IEnumerable<LungCare.SupportPlatform.Models.GetUserInfoResponse> list = listView.Items.SourceCollection.Cast<LungCare.SupportPlatform.Models.GetUserInfoResponse>();
            var items = list.First(item => ((LungCare.SupportPlatform.Models.GetUserInfoResponse)item).PhoneNumber == phoneNumber);
            if (items != null)
            {
                LungCare_Airway_PlanNav.UserCertificateCheckWindow certificateWindow = new LungCare_Airway_PlanNav.UserCertificateCheckWindow(items);
                certificateWindow.UpdateHandler += certificateWindow_UpdateHandler;
                certificateWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("未找到详细信息");
            }
        }

        void certificateWindow_UpdateHandler(object sender, RoutedEventArgs e)
        {
            if (UpdateHandler != null)
            {
                UpdateHandler(sender ,e);
            }
            //throw new NotImplementedException();
        }

        private void btn_approveLeft_Click(object sender, RoutedEventArgs e)
        {
            _approveCurrentPage -= 1;
            checkPage();
            LoadPage(_approveCurrentPage, _approvePageSize, _Allapprove);

        }

        private void btn_approveRight_Click(object sender, RoutedEventArgs e)
        {
            _approveCurrentPage += 1;
            checkPage();
            LoadPage(_approveCurrentPage, _approvePageSize, _Allapprove);
        }


        public void LoadPage(int currentPage, int pageSize, List<GetUserInfoResponse> _allapprove)
        {

            if (currentPage < _approveMaxPage)
            {
                List<GetUserInfoResponse> _CurrentPageapproveList = new List<GetUserInfoResponse>();

                for (int i = 0; i < pageSize; i++)
                {
                    _CurrentPageapproveList.Add(_allapprove[currentPage * pageSize + i]);
                }
                listView.DataContext = null;
                listView.DataContext = new System.ComponentModel.BindingList<GetUserInfoResponse>(_CurrentPageapproveList);
                listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());

            }
            if (currentPage == _approveMaxPage)
            {
                List<GetUserInfoResponse> _CurrentPageapproveList = new List<GetUserInfoResponse>();
                for (int i = 0; i < _allapprove.Count() % pageSize; i++)
                {
                    _CurrentPageapproveList.Add(_allapprove[currentPage * pageSize + i]);
                }
                listView.DataContext = null;
                listView.DataContext = new System.ComponentModel.BindingList<GetUserInfoResponse>(_CurrentPageapproveList);
                listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
            }

        }



        public void checkPage()
        {
            if (_approveCurrentPage <= 0)
            {
                btn_approveLeft.IsEnabled = false;
            }
            else
            {
                btn_approveLeft.IsEnabled = true;
            }
            if (_approveCurrentPage >= (_approveMaxPage))
            {
                btn_approveRight.IsEnabled = false;
            }
            else
            {

                btn_approveRight.IsEnabled = true;
            }
        }

	}
}