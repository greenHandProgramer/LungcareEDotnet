using AirwayCT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LungCare.SupportPlatform.UI.Windows.Examination
{
    /// <summary>
    /// StartWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
            PatientMgrForm form = new PatientMgrForm();
            form.Load += delegate
            {
                this.Hide();
            };

            form.ShowDialog();
        }
    }
}
