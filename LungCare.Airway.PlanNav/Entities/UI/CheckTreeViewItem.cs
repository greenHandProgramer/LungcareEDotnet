using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LungCare.SupportPlatform.Entities.UI
{
    class CheckTreeViewItem : TreeViewItem
    {
        private CheckBox _header = new CheckBox();
        public CheckTreeViewItem()
        {
            base.Header = _header;
            _header.Click += new RoutedEventHandler(OnClickHandle);
        }
        public object Header
        {
            get
            {
                return _header.Content;
            }
            set
            {
                _header.Content = value;
            }
        }



        #region Add the Click Event
        //Add a Click Event
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
          "Click",
          RoutingStrategy.Bubble,
          typeof(RoutedEventHandler),
          typeof(CheckTreeViewItem));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        protected void RaiseAddClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(CheckTreeViewItem.ClickEvent);
            RaiseEvent(newEventArgs);
        }

        //objects events handle
        private void OnClickHandle(object sender, System.Windows.RoutedEventArgs e)
        {
            RaiseAddClickEvent();
        }
        #endregion
    }
}
