using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LungCare.SupportPlatform.UI.Windows.Common
{
    /// <summary>
    /// VideoPlayer.xaml 的交互逻辑
    /// </summary>
    public partial class VideoPlayer : Window
    {
        private string _filename;
        public VideoPlayer(string filename)
        {
            InitializeComponent();

            this._filename = filename;
            this.SourceInitialized += VideoPlayer_SourceInitialized;
            this.SizeChanged += VideoPlayer_SizeChanged;
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;// 获得窗体的 样式

            int oldstyle = MedSys.PresentationCore.AdjustWindow.NativeMethods.GetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE, oldstyle & ~MedSys.PresentationCore.AdjustWindow.NativeMethods.WS_CAPTION);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetLayeredWindowAttributes(hwnd, 1 | 2 << 8 | 3 << 16, 0, MedSys.PresentationCore.AdjustWindow.NativeMethods.LWA_ALPHA);

            this.RefreshRoundWindowRect();

            headBorder.MouseDown += headBorder_MouseDown;

            Play(filename);
        }

        void VideoPlayer_SourceInitialized(object sender, EventArgs e)
        {
            this.MaxWidth = SystemParameters.WorkArea.Width;
            this.MaxHeight = SystemParameters.WorkArea.Height;
            //throw new NotImplementedException();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            MedSys.PresentationCore.AdjustWindow.ChangeWindowSize changeWindowSize = new MedSys.PresentationCore.AdjustWindow.ChangeWindowSize(this);
            changeWindowSize.RegisterHook();
        }

        void VideoPlayer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.RefreshRoundWindowRect();
            //throw new NotImplementedException();
        }

        private void RefreshRoundWindowRect()
        {
            // 获取窗体句柄
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

            // 创建圆角窗体
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowRgn(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.CreateRoundRectRgn(0, 0, Convert.ToInt32(this.ActualWidth) + 1, Convert.ToInt32(this.ActualHeight) + 1, 5, 5), true);
        }
        void headBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            //throw new NotImplementedException();
        }


        void timer_Tick(object sender, EventArgs e)
        {
            // Check if the movie finished calculate it's total time
            if (mediaElement.NaturalDuration.TimeSpan.TotalSeconds > 0)
            {
                if (TotalTime.TotalSeconds > 0)
                {
                    if (btnVideoStart.IsChecked == true)
                    {
                        // Updating time slider
                        slider.Value = mediaElement.Position.TotalSeconds * 30;
                    }

                    lblStatus.Content = String.Format("{0} / {1}", mediaElement.Position.ToString(@"hh\:mm\:ss"), mediaElement.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss"));
                }
            }
        }

        private void btnvideostretch_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal)
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }

        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (timerVideoTime != null)
            {
                timerVideoTime.Stop();
            }
            this.Close();
        }

        private void Play(string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                MessageBox.Show("文件不存在！");
                Close();
                return;
            }
            try
            {
                Console.WriteLine("Opening " + filename);
                mediaElement.Source = new Uri(filename);
                mediaElement.ScrubbingEnabled = true;
                mediaElement.MediaOpened += mediaElement_MediaOpened;
                mediaElement.MediaEnded += mediaElement_MediaEnded;
                btnVideoStart.IsChecked = true;

                mediaElement.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show("无效的视频文件！" + ex.Message);
                Close();
            }
        }

        void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
        }

        private TimeSpan TotalTime;
        private DispatcherTimer timerVideoTime;

        void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("mediaElement_MediaOpened");

            TotalTime = mediaElement.NaturalDuration.TimeSpan;

            slider.Maximum = TotalTime.TotalSeconds * 30;

            // Create a timer that will update the counters and the time slider
            timerVideoTime = new DispatcherTimer();
            timerVideoTime.Interval = TimeSpan.FromSeconds(0.1);
            timerVideoTime.Tick += new EventHandler(timer_Tick);
            timerVideoTime.Start();
        }

        private void Pause()
        {
            if (mediaElement.Source != null)
            {
                mediaElement.Pause();
            }
        }

        private void btnVideoStart_Checked(object sender, RoutedEventArgs e)
        {
            if (mediaElement.Source != null)
            {
                mediaElement.Play();
            }
        }

        private void btnVideoStart_Unchecked(object sender, RoutedEventArgs e)
        {
            if (mediaElement.Source != null)
            {
                mediaElement.Pause();
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (btnVideoStart.IsChecked == false)
            {
                if (TotalTime.TotalSeconds > 0)
                {
                    mediaElement.Position = TimeSpan.FromSeconds(slider.Value / 30);
                }
            }
        }

        private void timeSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (TotalTime.TotalSeconds > 0)
            {
                mediaElement.Position = TimeSpan.FromSeconds(slider.Value / 30);
            }
            btnVideoStart.IsChecked = isPauseBeforeMouseDownOnSlider;
        }

        bool isPauseBeforeMouseDownOnSlider = false;

        private void timeSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPauseBeforeMouseDownOnSlider = btnVideoStart.IsChecked.Value;
            btnVideoStart.IsChecked = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            slider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(timeSlider_MouseLeftButtonUp), true);
            slider.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(timeSlider_MouseLeftButtonDown), true);
        }

        private void slider_MouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}
