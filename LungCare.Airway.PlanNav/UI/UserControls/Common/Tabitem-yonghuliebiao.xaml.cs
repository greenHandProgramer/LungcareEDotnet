using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LungCare.SupportPlatform.Models;
using LungCare.SupportPlatform.WebAPIWorkers;

namespace LungCare.SupportPlatform.UI.UserControls.Common
{
	/// <summary>
	/// Tabitem_yonghuliebiao.xaml 的交互逻辑
	/// </summary>
	public partial class Tabitem_yonghuliebiao : UserControl
	{
		public Tabitem_yonghuliebiao()
		{
			this.InitializeComponent();
		}

        GetUserInfoResponse[] _Alluser = new GetUserInfoResponse[]{};
        GetUserInfoResponse[] _tempUserList = new GetUserInfoResponse[] { };
        public event EventHandler<RoutedEventArgs> DeleteHandler;

        private int _MinPage = 0;
        private int _MaxPage = 0;
        private int _CurrentPage = 0;
        private int _PageSize = 13;


        public void SetDataSource(GetUserInfoResponse[] dataSource)
        {
            _Alluser = dataSource;
            _tempUserList = _Alluser;
            //_CurrentPage = 0;
            if (_tempUserList.Count() % _PageSize == 0)
            {
                _MaxPage = _tempUserList.Count() / _PageSize - 1;
            }
            else
            {
                _MaxPage = _tempUserList.Count() / _PageSize;
            }
            checkPage(_CurrentPage,_MaxPage,_MinPage);
            LoadPage(_CurrentPage, _PageSize, _tempUserList, _MaxPage);
        }

        private void detailLinkLabel_Click(object sender, RoutedEventArgs e)
        {
            ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
            if (LinkLabel.Tag==null) {
                MessageBox.Show("数据为空,无法删除");
                return;
            }
            string phoneNumber = LinkLabel.Tag.ToString();
            Console.WriteLine(LinkLabel.Tag.ToString());

            IEnumerable<LungCare.SupportPlatform.Models.GetUserInfoResponse> list = listView.Items.SourceCollection.Cast<LungCare.SupportPlatform.Models.GetUserInfoResponse>();
            var items = list.First(item => ((LungCare.SupportPlatform.Models.GetUserInfoResponse)item).PhoneNumber == phoneNumber);
            if (items != null)
            {
                LungCare_Airway_PlanNav.UserWindow userWindow = new LungCare_Airway_PlanNav.UserWindow(items);
                userWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("未找到详细信息");
            }
        }

        private void btn_userLeft_Click(object sender, RoutedEventArgs e)
        {
            _CurrentPage -= 1;
            checkPage(_CurrentPage,_MaxPage,_MinPage);
            LoadPage(_CurrentPage, _PageSize, _tempUserList, _MaxPage);

        }

        private void btn_userRight_Click(object sender, RoutedEventArgs e)
        {
            _CurrentPage += 1;
            checkPage(_CurrentPage,_MaxPage,_MinPage);
            LoadPage(_CurrentPage, _PageSize, _tempUserList, _MaxPage);
        }


        public void LoadPage(int currentPage, int pageSize, GetUserInfoResponse[] _alluser,int maxPage)
        {
            Array.Sort(_alluser, (t1, t2) => DateTime.Parse(t2.RegistrationTimeStamp).CompareTo(DateTime.Parse(t1.RegistrationTimeStamp)));
            label_totalNumber.Content = "用户总数：" + _alluser.Length;
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            label_lastRefreshTime.Content = "最后刷新的时间是：" + now;
            label_Pages.Content = "第" + (currentPage + 1) + "页  共" + (maxPage + 1) + "页";
            if (currentPage < maxPage)
            {
                GetUserInfoResponse[] _CurrentPageuserList = new GetUserInfoResponse[pageSize];
                for (int i = 0; i < pageSize; i++)
                {
                    _CurrentPageuserList[i] = _alluser[currentPage * pageSize + i];
                }
                listView.DataContext = null;
                listView.DataContext = new System.ComponentModel.BindingList<GetUserInfoResponse>(_CurrentPageuserList);
                listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
            }
            if (currentPage == maxPage)
            {
                if (_alluser.Count() % pageSize!=0) {
                
                    GetUserInfoResponse[] _CurrentPageuserList = new GetUserInfoResponse[_alluser.Count() % pageSize];
                    for (int i = 0; i < _alluser.Count() % pageSize; i++)
                    {
                        _CurrentPageuserList[i] = _alluser[currentPage * pageSize + i];
                    }
                    listView.DataContext = null;
                    listView.DataContext = new System.ComponentModel.BindingList<GetUserInfoResponse>(_CurrentPageuserList);
                    listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
                }
                if (_alluser.Count() % pageSize == 0) {
                    GetUserInfoResponse[] _CurrentPageuserList = new GetUserInfoResponse[pageSize];
                    for (int i = 0; i < pageSize; i++)
                    {
                        _CurrentPageuserList[i] = _alluser[currentPage * pageSize + i];
                    }
                    listView.DataContext = null;
                    listView.DataContext = new System.ComponentModel.BindingList<GetUserInfoResponse>(_CurrentPageuserList);
                    listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
                }
            }

        }



        public void checkPage(int currentPage,int maxPage,int minPage)
        {
            if (currentPage <= minPage)
            {
                btn_userLeft.IsEnabled = false;
            }
            else
            {
                btn_userLeft.IsEnabled = true;
            }
            if (currentPage >= (maxPage))
            {
                btn_userRight.IsEnabled = false;
            }
            else
            {
                btn_userRight.IsEnabled = true;
            }
        }

        private void deleteUserLinkLabel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult MsgBoxResult;
            MsgBoxResult = MessageBox.Show("是否删除用户？", "提示", MessageBoxButton.OKCancel);
            if (MsgBoxResult == MessageBoxResult.Cancel)
            {
                return;
            }
            else { 
                ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
                string phoneNumber = LinkLabel.Tag.ToString();
                Console.WriteLine(LinkLabel.Tag.ToString());

                IEnumerable<LungCare.SupportPlatform.Models.GetUserInfoResponse> list = listView.Items.SourceCollection.Cast<LungCare.SupportPlatform.Models.GetUserInfoResponse>();
                var items = list.First(item => ((LungCare.SupportPlatform.Models.GetUserInfoResponse)item).PhoneNumber == phoneNumber);
                if (items != null)
                {
                    String userID = items.PhoneNumber;
                    DeleteUserWorker.SendDeleteUserRequest(userID,
                    successCallback:
                    delegate {
                        if(DeleteHandler!=null)
                        {
                            DeleteHandler(sender , e);
                        }
                        MessageBox.Show("delete success!!!");
                            
                    },
                    failureCallback: delegate { },
                    errorCallback: delegate { });
                }
                else
                {
                    MessageBox.Show("未找到详细信息");
                }
            }
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text2Search = tbSearch.Text.ToLower();

            if (_Alluser == null || _Alluser.Length == 0)
            {
                return;
            }

            _tempUserList = _Alluser.Where(item => ((item.ChineseName != null && item.ChineseName.ToLower().Contains(text2Search)) || (item.PhoneNumber != null && item.PhoneNumber.ToLower().Contains(text2Search)) || (item.Institution != null && item.Institution.ToLower().Contains(text2Search)))).ToArray();
            
            _CurrentPage = 0;
            if (_tempUserList.Count() % _PageSize == 0)
            {
                _MaxPage = _tempUserList.Count() / _PageSize - 1;
            }
            else
            {
                _MaxPage = _tempUserList.Count() / _PageSize;
            }
            checkPage(_CurrentPage, _MaxPage, _MinPage);
            LoadPage(_CurrentPage, _PageSize, _tempUserList,_MaxPage);
        }
    }
}
