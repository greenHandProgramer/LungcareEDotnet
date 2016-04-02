using LungCare.SupportPlatform.Entities;
using LungCare.SupportPlatform.UI;
using LungCare.SupportPlatform.UI.Windows.Examination;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LungCare.SupportPlatform.UI.UserControls.Examination
{
    /// <summary>
    /// LesionButtonUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class LesionButtonUserControl : UserControl
    {
        private int _index = 0;
        private double[] _position;
        private string saveFolder;
        public bool isSelect = false;
        public event EventHandler<EventArgs> StartSnapshotEvent;
        public AirwayCT.Entity.AirwayPatient AirwayPatient;
        public LesionEntity _lesion;
        public LesionButtonUserControl()
        {
            InitializeComponent();
        }

        public LesionButtonUserControl(int index)
        {
            InitializeComponent();
            _index = index;
            btn.Content = _index;
        }
        public LesionButtonUserControl(int index ,double[] position)
        {
            InitializeComponent();
            _index = index;
            _position = position;
            btn.Content = _index;
        }

        public LesionButtonUserControl(int index, LesionEntity lesion)
        {
            InitializeComponent();
            _index = index;
            _lesion = lesion;
            _position = lesion.position;
            btn.Content = _index;
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }



        private void AddNewLesion()
        {
            
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            SnapshotWindow snapshot = new SnapshotWindow(AirwayPatient , _index , _lesion);
            snapshot.Topmost = true;
            
            snapshot.Loaded += delegate
            {
                if (StartSnapshotEvent != null)
                {
                    StartSnapshotEvent(this, new EventArgs());
                }
            };
            snapshot.Show();
        }



        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            isSelect = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isSelect = false;
        }
    }
}
