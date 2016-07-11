using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Threading;

namespace SyncPaint.page
{
    /// <summary>
    /// AlbumPage.xaml 的交互逻辑
    /// </summary>
    public partial class AlbumPage : Page
    {
        List<InkCanvas> inkcanvaslist = new List<InkCanvas>();
        List<string> inkcanvaspath = new List<string>();
        int inkcanvasCursor = 0;
        public AlbumPage()
        {
            InitializeComponent();
            loadStrokelist();
        }

        private void nextpicture_Click(object sender, RoutedEventArgs e)
        {
            if (inkcanvasCursor < inkcanvaspath.Count-1)
            {
                FileStream file = new FileStream(inkcanvaspath[++inkcanvasCursor], FileMode.Open, FileAccess.Read);
                ViewInkcanvas.Strokes = new StrokeCollection(file);
            }
            else {
                MessageBox.Show("已经是最后一张");
            }
        }

        private void prevpicture_Click(object sender, RoutedEventArgs e)
        {
            if (inkcanvasCursor > 0)
            {
                FileStream file = new FileStream(inkcanvaspath[--inkcanvasCursor], FileMode.Open, FileAccess.Read);
                ViewInkcanvas.Strokes = new StrokeCollection(file);
            }
            else {
                MessageBox.Show("已经是第一张");
            }
        }
        private void loadStrokelist()
        {
            DirectoryInfo TheFolder = new DirectoryInfo(Environment.CurrentDirectory + "\\strokeSave");
            try
            {
                foreach (FileInfo NextFile in TheFolder.GetFiles())
                {
                    if (NextFile.Extension == ".isf")
                    {
                        InkCanvas myinkcanvas = new InkCanvas();
                        
                        string path = NextFile.DirectoryName + "\\" + NextFile.Name;
                        Tool.Log(" 正在加载文件 "+path);
                        inkcanvaspath.Add(path);
                        FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                        StrokeCollection strokeCollection = new StrokeCollection(file);

                        Rect rectBounds = strokeCollection.GetBounds();
                        double heightscale = 100 / rectBounds.Height;

                        foreach (Stroke s in strokeCollection)
                        {
                            Matrix ScaleMatrix = new Matrix();
                            ScaleMatrix.Scale(heightscale, heightscale);
                            Matrix TranslateMatrix = new Matrix();
                            TranslateMatrix.Translate(-rectBounds.Left + 50, -rectBounds.Top + 50);
                            s.Transform(TranslateMatrix, false);
                            s.Transform(ScaleMatrix, false);
                        }

                        myinkcanvas.Width = rectBounds.Width * heightscale + 50;
                        myinkcanvas.Height = 170;
                        myinkcanvas.Strokes.Add(strokeCollection);
                        myinkcanvas.EditingMode = InkCanvasEditingMode.None;
                        inkcanvaslist.Add(myinkcanvas);
                    }
                }
                inklistview.ItemsSource = inkcanvaslist;
                if (inkcanvasCursor != 0) {
                    FileStream file = new FileStream(inkcanvaspath[0], FileMode.Open, FileAccess.Read);
                    ViewInkcanvas.Strokes = new StrokeCollection(file);
                }
            }
            catch (Exception ex)
            {
                DirectoryInfo TheFolder1 = new DirectoryInfo(Environment.CurrentDirectory + "\\strokeSave");
                MessageBox.Show("加载失败，请检查"+TheFolder.ToString()+"是否被删除");
                Debug.WriteLine(ex.Message);
            }

        }

        private void inklistview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            inkcanvasCursor = inklistview.SelectedIndex;
            //string path = Environment.CurrentDirectory + "\\strokeSave\\a" + i + ".isf";
            FileStream file = new FileStream(inkcanvaspath[inkcanvasCursor], FileMode.Open, FileAccess.Read);
            ViewInkcanvas.Strokes = new StrokeCollection(file);
        }

        private void btnReview_Click(object sender, RoutedEventArgs e)
        {
            
            
         
            StrokeCollection strokecollection = new StrokeCollection();
            strokecollection = ViewInkcanvas.Strokes.Clone();
            ViewInkcanvas.Strokes.Clear();
            ThreadPool.QueueUserWorkItem((o) =>
            {
                foreach (Stroke s in strokecollection)
                {
                    Thread.Sleep(100);
                    
                    ViewInkcanvas.Dispatcher.Invoke(new Action(() =>
                    {
                        ViewInkcanvas.Strokes.Add(s);
                    }));
                }
            });


        }

        private void reviewThread()
        {
            StrokeCollection strokecollection = new StrokeCollection();
            int i = 1;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                strokecollection = ViewInkcanvas.Strokes.Clone();
                ViewInkcanvas.Strokes.Clear();

                foreach (Stroke s in strokecollection)
                {
                    Thread.Sleep(100);

                    ViewInkcanvas.Dispatcher.Invoke(new Action(() =>
                    {
                        ViewInkcanvas.Strokes.Add(s);
                    }));
                }
            });
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (CmdManager.main == null)
            {
                MainWindow main = new MainWindow();
                main.Show();
            }
            else
            {
                CmdManager.main.Visibility = Visibility.Visible;
            }

        }

        private void testload_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo TheFolder = new DirectoryInfo(Environment.CurrentDirectory + "\\strokeSave");
            MessageBox.Show(TheFolder.ToString());
        }
    }
}
