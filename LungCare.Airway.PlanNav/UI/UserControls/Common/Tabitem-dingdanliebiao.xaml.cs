using LungCare.SupportPlatform.Models;
using System;
using System.Linq;
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
using System.Threading;
using LungCare.SupportPlatform.WebAPIWorkers;

namespace LungCare.SupportPlatform.UI.UserControls.Common
{
	/// <summary>
	/// Tabitem_dingdanliebiao.xaml 的交互逻辑
	/// </summary>
	public partial class Tabitem_dingdanliebiao : UserControl
	{
		public Tabitem_dingdanliebiao()
		{
			this.InitializeComponent();
		}


        private int _MinPage = 0;
        private int _MaxPage = 0;
        private int _CurrentPage = 0;
        private int _PageSize = 12;
        private DataListItem[] _AllOrder = new DataListItem[]{};
        private DataListItem[] _tempOrderList = new DataListItem[] { };
        public event EventHandler<RoutedEventArgs> DeleteHandler;

        public void SetDataSource(DataListItem[] dataSource)
        {
            _AllOrder = dataSource;
            _tempOrderList = _AllOrder;
            //_CurrentPage = 0;
            if (_tempOrderList.Count() % _PageSize == 0)
            {
                _MaxPage = _tempOrderList.Count() / _PageSize - 1;
            }
            else {
                _MaxPage = _tempOrderList.Count() / _PageSize;
            }
            checkPage(_CurrentPage, _MaxPage,_MinPage);
            LoadPage(_CurrentPage, _PageSize, _tempOrderList,_MaxPage);
            
        }

        private void detailLinkLabel_Click(object sender, RoutedEventArgs e)
        {
            
            ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
            if (LinkLabel.Tag == null)
            {
                MessageBox.Show("数据为空，无法查看详情");
                return;
            }
            string UploadTimestamp = LinkLabel.Tag.ToString();
            Console.WriteLine(LinkLabel.Tag.ToString());

            IEnumerable<LungCare.SupportPlatform.Models.DataListItem> list = listView.Items.SourceCollection.Cast<LungCare.SupportPlatform.Models.DataListItem>();
            var items = list.First(item => ((LungCare.SupportPlatform.Models.DataListItem)item).UploadTimestamp == UploadTimestamp);
            if (items != null)
            {
                LungCare_Airway_PlanNav.OrderItemWindow orderWindow = new LungCare_Airway_PlanNav.OrderItemWindow(items);
                orderWindow.updateHandler += orderWindow_updateHandler;
                orderWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("未找到详细信息");
            }  

        }

        void orderWindow_updateHandler(object sender, RoutedEventArgs e)
        {
            if(DeleteHandler!=null){
                DeleteHandler(sender,e);
            }
        }

        private void btn_orderLeft_Click(object sender, RoutedEventArgs e)
        {
            _CurrentPage -= 1;
            checkPage(_CurrentPage, _MaxPage,_MinPage);
            LoadPage(_CurrentPage,_PageSize,_tempOrderList,_MaxPage);

        }

        private void btn_orderRight_Click(object sender, RoutedEventArgs e)
        {
            _CurrentPage += 1;
            checkPage(_CurrentPage,_MaxPage,_MinPage);
            LoadPage(_CurrentPage, _PageSize, _tempOrderList,_MaxPage);
        }
        

        public void LoadPage(int currentPage, int pageSize, DataListItem[] _allOrder,int maxPage) {
            label_totalNumber.Content = "订单总数：" + _allOrder.Length;
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            label_lastRefreshTime.Content = "最后刷新的时间是：" + now;
            label_Pages.Content = "第" + (currentPage + 1) + "页  共" + (maxPage + 1) + "页";  
            if (currentPage < maxPage)
            {
                DataListItem[] _CurrentPageOrderList = new DataListItem[pageSize];
                for (int i = 0; i < pageSize; i++)
                {
                    _CurrentPageOrderList[i] = _allOrder[currentPage*pageSize+i];
                }
                listView.DataContext = new System.ComponentModel.BindingList<DataListItem>(_CurrentPageOrderList);
                listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
            }
            if (currentPage == maxPage)
            {
                if (_allOrder.Count() % pageSize!=0) {
                
                    DataListItem[] _CurrentPageOrderList = new DataListItem[_allOrder.Count() % pageSize];
                    for (int i = 0; i < _allOrder.Count() % pageSize; i++)
                    {
                        _CurrentPageOrderList[i] = _allOrder[currentPage * pageSize + i];
                    }
                    listView.DataContext = null;
                    listView.DataContext = new System.ComponentModel.BindingList<DataListItem>(_CurrentPageOrderList);
                    listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
                }

                if (_allOrder.Count() % pageSize == 0) {
                    DataListItem[] _CurrentPageOrderList = new DataListItem[pageSize];
                    for (int i = 0; i < pageSize; i++)
                    {
                        _CurrentPageOrderList[i] = _allOrder[currentPage * pageSize + i];
                    }
                    listView.DataContext = null;
                    listView.DataContext = new System.ComponentModel.BindingList<DataListItem>(_CurrentPageOrderList);
                    listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
                }
            }
        }

        
        /// <summary>
        /// 根据当前page与minPage和maxPage的比较设定上下页按钮的状态
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="maxPage"></param>
        public void checkPage(int currentPage,int maxPage,int minPage) {
            if (currentPage <= minPage)
            {
                btn_orderLeft.IsEnabled = false;
            }
            else {
                btn_orderLeft.IsEnabled = true;
            }
            if (currentPage >= (maxPage))
            {
                btn_orderRight.IsEnabled = false;
            }
            else {
                btn_orderRight.IsEnabled = true;
            }
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteOrderLinkLabel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult MsgBoxResult;
            MsgBoxResult = MessageBox.Show("是否删除订单？", "提示", MessageBoxButton.OKCancel);
            if (MsgBoxResult == MessageBoxResult.Cancel)
            {
                return;
            }
            else
            {
                ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
                if (LinkLabel.Tag == null)
                {
                    MessageBox.Show("数据为空，无法删除");
                    return;
                }
                string UploadTimestamp = LinkLabel.Tag.ToString();

                Console.WriteLine(LinkLabel.Tag.ToString());

                IEnumerable<LungCare.SupportPlatform.Models.DataListItem> list = listView.Items.SourceCollection.Cast<LungCare.SupportPlatform.Models.DataListItem>();
                var items = list.First(item => ((LungCare.SupportPlatform.Models.DataListItem)item).UploadTimestamp == UploadTimestamp);
                if (items != null)
                {
                    String DataID = items.DataID;
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        CancelOrderWorker.SendCancelOrderRequeset(
                            DataID,
                            successCallback: delegate(CancelOrderResponse response)
                            {
                                if (DeleteHandler != null)
                                {
                                    DeleteHandler(sender, e);
                                }
                                Dispatcher.Invoke(new Action(delegate
                                {
                                    MessageBox.Show("取消订单成功。DataId : " + DataID, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                                }));
                            },
                            failureCallback: delegate(string failureReason)
                            {
                                Dispatcher.Invoke(new Action(delegate
                                {
                                    MessageBox.Show("取消订单失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                                }));
                            },
                            errorCallback: delegate(Exception ex)
                            {
                                Dispatcher.Invoke(new Action(delegate
                                {
                                    Util.ShowExceptionMessage(ex, "取消订单出错。");
                                }));
                            });
                    });
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

            if (_AllOrder == null || _AllOrder.Length == 0)
            {
                return;
            }

            _tempOrderList = _AllOrder.Where(item => ((item.ChineseName!=null&&item.ChineseName.ToLower().Contains(text2Search))||(item.UserId !=null&&item.UserId.ToLower().Contains(text2Search)) ||(item.PatientName!=null&&item.PatientName.ToLower().Contains(text2Search)) ||(item.DataID!=null&&item.DataID.ToLower().Contains(text2Search)))).ToArray();
            _CurrentPage = 0;
           
            
            if (_tempOrderList.Count() % _PageSize == 0)
            {
                _MaxPage = _tempOrderList.Count() / _PageSize - 1;
            }
            else
            {
                _MaxPage = _tempOrderList.Count() / _PageSize;
            }

            checkPage(_CurrentPage, _MaxPage, _MinPage);
            LoadPage(_CurrentPage, _PageSize, _tempOrderList,_MaxPage);
        }
        
	}
}