using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Net;

using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using ZXing.Common;
using ZXing;
using ZXing.QrCode;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace SyncPaint
{
    /// <summary>
    /// ErcodePage.xaml 的交互逻辑
    /// </summary>
    /// 
    
    public partial class ErcodePage : Page
    {
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);
        string oldip = null;
        System.Timers.Timer t = new System.Timers.Timer(400);
        public ErcodePage()
        {
            InitializeComponent();
            oldip = GetAddressIP();
            imageCode.Source = createQRCode(GetAddressIP(), 250, 250);//在界面上显示二维码
            openService();
            Init();
        }

        private void Init()
        {
             
            t.Elapsed += new System.Timers.ElapsedEventHandler(theout);  
            t.AutoReset = true;   
            t.Enabled = true;
        }

        public void theout(object source, System.Timers.ElapsedEventArgs e)
        {
            string newip = GetAddressIP();
            if (!(newip == oldip) && newip != "127.0.0.1")
            {
                oldip = newip;
                Console.WriteLine("检测到ip发生了变化" + newip);
                Dispatcher.Invoke(new Action(() =>
                {
                    imageCode.Source = createQRCode(newip, 250, 250);//在界面上显示二维码
                }));
                openService();
            }
        }

        public void openService()
        {
            string AddressIP = string.Empty;
            //获取本机ip
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            CmdManager.IP = AddressIP;
            CmdManager.Ercode = this;
            CmdManager.socketStart();
        }

        private ImageSource createQRCode(string content, int width, int height)
        {
            EncodingOptions options;
            BarcodeWriter write = null;
            options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = width,
                Height = height,
                Margin = 0
            };
            write = new BarcodeWriter();
            //设置二维码颜色
            //write.Renderer = new ZXing.Rendering.BitmapRenderer { Background = System.Drawing.Color.White, Foreground = System.Drawing.Color.Aqua };
            write.Format = BarcodeFormat.QR_CODE;
            write.Options = options;
            Bitmap bitmap = write.Write(content);
            IntPtr ip = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ip);
            return bitmapSource;
        }


        private string GetAddressIP()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }

        
        public void changePage()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                NavigationWindow window = new NavigationWindow();
                window.Source = new Uri("page/DrawPage.xaml", UriKind.Relative);
                window.WindowState = WindowState.Maximized;
                window.Left = 0.0;
                window.Top = 0.0;
                window.Title = "同步手绘板";
                window.ShowInTaskbar = true;
                window.Show();
                Tool.Log("changePage");
            
                Window win = (Window)this.Parent;
                win.Close();
            });
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

            //CmdManager.closrListener();

            t.Close();
            if (CmdManager.isConnect == false)
            {
                MainWindow main = new MainWindow();
                main.Show();

                CmdManager.listener.Stop();
                //CmdManager.tcpClient.Close();
                Tool.Log((CmdManager.tcpClient == null) + "★   测试  ");
               
                CmdManager.tcpClient = null;
            }
            
        }
    }
}
