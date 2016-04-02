using LungCare.SupportPlatform.Entities;
using LungCare.SupportPlatform.Network;
using LungCare.SupportPlatform.UI.Windows.Common;
using LungCare.SupportPlatform.WebAPIWorkers;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace LungCare.SupportPlatform.UI.Windows.Service
{
    /// <summary>
    /// MsgWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ServiceListWnd : Window
    {

        public ServiceListWnd()
        {
            InitializeComponent();
            borderMessage.MouseDown+=borderMessage_MouseDown;
            FooViewModel root = this.tree.Items[0] as FooViewModel;

            base.CommandBindings.Add(
                new CommandBinding(
                    ApplicationCommands.Undo,
                    (sender, e) => // Execute
                    {
                        e.Handled = true;
                        root.IsChecked = false;
                        this.tree.Focus();
                    },
                    (sender, e) => // CanExecute
                    {
                        e.Handled = true;
                        e.CanExecute = (root.IsChecked != false);
                    }));

            base.CommandBindings.Add(
              new CommandBinding(
                  ApplicationCommands.Redo,
                  (sender, e) => // Execute
                  {
                      e.Handled = true;
                      root.IsChecked = true;
                      this.tree.Focus();
                  },
                  (sender, e) => // CanExecute
                  {
                      e.Handled = false;
                      e.CanExecute = true;
                  }));
            this.tree.Focus();
        }



        public bool IsWin7System
        {
            get { return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major >= 6); }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsWin7System)
            {
                //btnMin.Visibility = Visibility.Hidden;
            }


        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("是否关闭？", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //if(e.Key == Key.Escape)
            //{
            //    Close();
            //}
        }

        private void borderMessage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove(); 
            }
            catch (Exception)
            {
            }
            
        }


        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            //MainWindow.Instance.BringIntoView();
        }

        private void btnMin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
            //MainWindow.Instance.WindowState = System.Windows.WindowState.Minimized;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
