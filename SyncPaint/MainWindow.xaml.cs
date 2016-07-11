using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace SyncPaint
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void btn_newPaint_Click(object sender, RoutedEventArgs e)
        {
            if (CmdManager.tcpClient == null)
            {
                NavigationWindow window = new NavigationWindow();
                window.Source = new Uri("page/ErcodePage.xaml", UriKind.Relative);
                window.Title = "手机扫码";
                window.ResizeMode = ResizeMode.NoResize;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.ShowInTaskbar = true;
                window.Width = 600;
                window.Height = 500;
                window.Show();

                this.Visibility = System.Windows.Visibility.Hidden;
                Tool.Log(" null  ");
                //Close();

            }
            else
            {
                if (CmdManager.tcpClient.Connected)
                {
                    NavigationWindow window = new NavigationWindow();
                    window.Source = new Uri("page/ErcodePage.xaml", UriKind.Relative);
                    window.Title = "手机扫码";
                    window.ResizeMode = ResizeMode.NoResize;
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    window.ShowInTaskbar = true;
                    window.Width = 600;
                    window.Height = 500;
                    window.Show();

                    Tool.Log(" Connected ");

                }
                else
                {
                    NavigationWindow window = new NavigationWindow();
                    window.Source = new Uri("page/DrawPage.xaml", UriKind.Relative);
                    window.WindowState = WindowState.Maximized;
                    //window.Topmost = true;
                    window.Left = 0.0;
                    window.Top = 0.0;
                    window.Title = "同步手绘板";
                    window.ShowInTaskbar = true;
                    window.Show();
                    Window win = (Window)this.Parent;
                    win.Close();

                    Tool.Log(" unConnected ");
                }
                this.Visibility = System.Windows.Visibility.Hidden;

            }
        }

        private void btn_historyPaint_Click(object sender, RoutedEventArgs e)
        {
            NavigationWindow window = new NavigationWindow();
            window.Source = new Uri("page/AlbumPage.xaml", UriKind.Relative);
            //window.Topmost = true;
            window.Title = "我的作品";
          
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ShowInTaskbar = true;
            window.Show();
            this.Visibility = System.Windows.Visibility.Hidden;
            //Close();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //程序关闭后关闭所有线程
            Process.GetCurrentProcess().Kill();
        }
    }
}
