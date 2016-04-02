using LungCare.SupportPlatform.Models;
using LungCare.SupportPlatform.Network;
using LungCare.SupportPlatform.SupportPlatformDAO.LocalDicom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ThemedControlsLibrary;

namespace LungCare.SupportPlatform.SupportPlatformDAO.UI
{
    class ListViewDAO
    {

        public static void SetCheckStatusInListView(ListView listView ,string cbName , bool isCheck)
        {
            for (int i = 0; i < listView.Items.Count; i++)
            {
                listView.UpdateLayout();
                listView.ScrollIntoView(listView.Items[i]);
                ListViewItem item = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(i);
                CheckBox cb = Utils.FindChild<CheckBox>(item, cbName);
                LinkLabel linkLabelDicom = Utils.FindChild<LinkLabel>(item, "linkDicom");
                LinkLabel linkLabelAirway = Utils.FindChild<LinkLabel>(item, "linkLabelAirway");
                if (cb != null)
                {
                    if (linkLabelDicom != null)
                    {
                        string linkLabelDicomContent = (string)linkLabelDicom.Content;
                        string linkLabelAirwayContent = (string)linkLabelAirway.Content;
                        //Console.WriteLine(i + " " + linkLabelDicomContent);
                        if (linkLabelDicomContent=="下载" || linkLabelAirwayContent=="下载")
                        {
                            cb.IsChecked = isCheck;
                        }else
                        {
                            cb.IsChecked = false;
                        }
                    }
                  
                }
            }

            listView.ScrollIntoView(listView.Items[0]);
        }

        public static void SelectDownloadDicomCheckStatusInListViewExcept(ListView listView, string cbName)
        {
            for (int i = 0; i < listView.Items.Count; i++)
            {
                listView.UpdateLayout();
                listView.ScrollIntoView(listView.Items[i]);
                ListViewItem item = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(i);
                CheckBox cb = Utils.FindChild<CheckBox>(item, cbName);
                LinkLabel linkLabel = Utils.FindChild<LinkLabel>(item, "linkDicom");

                if (cb != null)
                {
                    if (linkLabel != null)
                    {
                        string linkLabelContent = (string)linkLabel.Content;
                        if (linkLabelContent == "下载")
                        {
                            cb.IsChecked = true;
                        }
                        else
                        {
                            cb.IsChecked = false;
                        }
                    }
                }

            }
        }


        public static void SelectDownloadAirwayCheckStatusInListViewExcept(ListView listView, string cbName)
        {
            for (int i = 0; i < listView.Items.Count; i++)
            {
                listView.UpdateLayout();
                listView.ScrollIntoView(listView.Items[i]);
                ListViewItem item = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(i);
                CheckBox cb = Utils.FindChild<CheckBox>(item, cbName);
                LinkLabel linkLabel = Utils.FindChild<LinkLabel>(item, "linkLabelAirway");

                if (cb != null)
                {
                    if (linkLabel != null)
                    {
                        string linkLabelContent = (string)linkLabel.Content;
                        if (linkLabelContent == "下载")
                        {
                            cb.IsChecked = true;
                        }
                        else
                        {
                            cb.IsChecked = false;
                        }
                    }
                }

            }
        }


        public static void SetCheckStatusInListViewItem(ListView listView ,int index ,string cbName , bool isCheck)
        {
            ListViewItem item = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(index);
            CheckBox cb = Utils.FindChild<CheckBox>(item, cbName);
            if (cb != null)
            {
                cb.IsChecked = true;
            }
        }


        public static List<DataListItem> SelectDataListItemsFromListView(ListView listView, string cbName, bool isCheck)
        {
            List<DataListItem> result = new List<DataListItem>();
            IEnumerable<Models.DataListItem> list = listView.Items.SourceCollection.Cast<Models.DataListItem>();
            for (int i = 0; i < listView.Items.Count; i++) 
            {
                ListViewItem listItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(i);
                CheckBox cb = Utils.FindChild<CheckBox>(listItem, cbName);
                LinkLabel linkLabel = Utils.FindChild<LinkLabel>(listItem, "linkDicom");
                if (cb != null)
                {

                    if (linkLabel != null )
                    {
                        string linkLabelContent = (string)linkLabel.Content;
                        if (linkLabelContent != "未上传")
                        {
                            if (cb.IsChecked.HasValue && cb.IsChecked.Value)
                            {
                                string dataID = cb.Content.ToString();

                                var items = list.First(item => ((Models.DataListItem)item).DataID == dataID);
                                result.Add(items);
                            }
                        }
                    }
                }
            }

            return result;

        }


        public static List<DataListItem> SelectDicomDataListItemsFromListView(ListView listView, string cbName, bool isCheck)
        {
            List<DataListItem> result = new List<DataListItem>();
            IEnumerable<Models.DataListItem> list = listView.Items.SourceCollection.Cast<Models.DataListItem>();
            for (int i = 0; i < listView.Items.Count; i++)
            {
                ListViewItem listItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(i);
                CheckBox cb = Utils.FindChild<CheckBox>(listItem, cbName);
                LinkLabel linkLabel = Utils.FindChild<LinkLabel>(listItem, "linkDicom");
                if (cb != null)
                {

                    if (linkLabel != null)
                    {
                        string linkLabelContent = (string)linkLabel.Content;
                        if (linkLabelContent == "下载")
                        {
                            if (cb.IsChecked.HasValue && cb.IsChecked.Value)
                            {
                                string dataID = cb.Content.ToString();

                                var items = list.First(item => ((Models.DataListItem)item).DataID == dataID);
                                result.Add(items);
                            }
                        }
                    }
                }
            }

            return result;

        }



        public static List<DataListItem> SelectAirwayDataListItemsFromListView(ListView listView, string cbName, bool isCheck)
        {
            List<DataListItem> result = new List<DataListItem>();
            IEnumerable<Models.DataListItem> list = listView.Items.SourceCollection.Cast<Models.DataListItem>();
            for (int i = 0; i < listView.Items.Count; i++)
            {
                ListViewItem listItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(i);
                CheckBox cb = Utils.FindChild<CheckBox>(listItem, cbName);
                LinkLabel linkLabel = Utils.FindChild<LinkLabel>(listItem, "linkLabelAirway");
                if (cb != null)
                {

                    if (linkLabel != null)
                    {
                        string linkLabelContent = (string)linkLabel.Content;
                        if (linkLabelContent == "下载")
                        {
                            if (cb.IsChecked.HasValue && cb.IsChecked.Value)
                            {
                                string dataID = cb.Content.ToString();

                                var items = list.First(item => ((Models.DataListItem)item).DataID == dataID);
                                result.Add(items);
                            }
                        }
                    }
                }
            }

            return result;

        }
        
    }
}
