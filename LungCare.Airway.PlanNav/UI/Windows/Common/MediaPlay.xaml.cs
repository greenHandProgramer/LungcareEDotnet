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
using System.Windows.Threading;

namespace LungCare.SupportPlatform.UI.Windows.Common
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MediaPlay : Window
    {
        public MediaPlay()
        {
            InitializeComponent();

            string filename = @"D:\project\navigate\LungCare.Airway.PlanNav\bin\Debug\download1.avi";
            mediaElement.Source = new Uri(filename);
            mediaElement.Play();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }


        public MediaPlay(string filename)
        {
            this.InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            mediaElement.Source = new Uri(tbFileName.Text);
            mediaElement.Play();
            
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.Source != null)
            {
                if (mediaElement.NaturalDuration.HasTimeSpan)
                {
                    lblStatus.Content = String.Format("{0} / {1}", mediaElement.Position.ToString(@"hh\:mm\:ss"), mediaElement.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss"));

                    if (!slider.IsEnabled)
                    {
                        slider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds - 1;
                        slider.IsEnabled = true;
                    }
                }
            }
            else
                lblStatus.Content = "No file selected...";
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            string filename = @"D:\project\navigate\LungCare.Airway.PlanNav\bin\Debug\download1.avi";
            mediaElement.Source = new Uri(filename);
            mediaElement.Play();


            return;
            if (tbFileName.Text.Length <= 0)
            {
                MessageBox.Show("Enter a valid media file");
                return;
            }

            mediaElement.Source = new Uri(tbFileName.Text);

            mediaElement.Play();

        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Position = TimeSpan.FromSeconds(slider.Value);
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Pause();
        }
    }
}
