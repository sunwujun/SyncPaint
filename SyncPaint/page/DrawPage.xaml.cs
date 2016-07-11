using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Ink;
using System.Threading;
using System.IO;
using SyncPaint.command;
using SyncPaint.stroke;
using Microsoft.Win32;
using System.Windows.Navigation;

namespace SyncPaint
{
    partial class DrawPage : Page
    {
        #region 参数定义

        private Stroke newStroke;

        //控制矩形框以及缩放位置相关变量
        private Rect orgin;
        private double thumbWidth;
        private double thumbHeight;
        private double oldThumbLeft;
        private double oldThumbTop;
        private double newThumbLeft;
        private double newThumbTop;
        private double mobileWidth;
        private double myRatio;
        private float thumbScaleMultiple = 4;

        public int size = 1;//画笔初始粗细
        private StrokeCollection LixianCollection = new StrokeCollection();//离线笔迹集合
        private DrawingAttributes myDrawingAttributes = new DrawingAttributes();//笔迹样式
        private SolidColorBrush mySolidColorBrush = new SolidColorBrush();//颜色选择
        private Color myColor = new Color();
        private CommandStack cmdStack;

        private bool IsLixian = false;
        private bool IsHidden = false;

        private NavigationWindow waitingWindow;
        #endregion 参数定义
        public DrawPage()
        {
            InitializeComponent();
            InIt();
        }

        #region 点击事件

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void btnReview_Click(object sender, RoutedEventArgs e)
        {
            ReviewStroke();
        }


        #endregion 点击事件

        #region 拖动事件

        private void DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            //开始拖动矩形选择区域事件
            thumb1.Background = Brushes.Aqua;
            thumb1.Opacity = 0.5;

            oldThumbTop = Canvas.GetTop(thumb1);
            oldThumbLeft = Canvas.GetLeft(thumb1);
            CmdManager.putCommand(new CameraChangeStartCommand());

        }

        int count = 0;
        private void DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            float dx = 0;
            float dy = 0;
            //正在拖动矩形选择区域事件
            newThumbTop = Canvas.GetTop(thumb1) + e.VerticalChange;
            newThumbLeft = Canvas.GetLeft(thumb1) + e.HorizontalChange;

            if (newThumbTop <= 0)
                newThumbTop = 0;
            if (newThumbTop >= (g.Height - thumb1.Height))
                newThumbTop = g.Height - thumb1.Height;
            if (newThumbLeft <= 0)
                newThumbLeft = 0;
            if (newThumbLeft >= (g.Width - thumb1.Width))
                newThumbLeft = g.Width - thumb1.Width;

            dx = (float)((oldThumbLeft - newThumbLeft) * thumbScaleMultiple / myRatio);
            dy = (float)((oldThumbTop - newThumbTop) * thumbScaleMultiple / myRatio);


            if (count == 8)//降低采样速度
            {
                count = 0;
                CmdManager.putCommand(new CameraChangeOnCommand(dx, dy, (float)myRatio));
            }
            count++;

            Canvas.SetTop(thumb1, newThumbTop);
            Canvas.SetLeft(thumb1, newThumbLeft);

        }

        private void DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            //结束拖动矩形框事件
            thumb1.Background = null;
            thumb1.Opacity = 0.2;


            //最终矩形框的位置
            newThumbTop = Canvas.GetTop(thumb1);
            newThumbLeft = Canvas.GetLeft(thumb1);
            //防止矩形框超出区域
            if (newThumbTop <= 0)
                newThumbTop = 0;
            if (newThumbTop >= (g.Height - thumb1.Height))
                newThumbTop = g.Height - thumb1.Height;
            if (newThumbLeft <= 0)
                newThumbLeft = 0;
            if (newThumbLeft >= (g.Width - thumb1.Width))
                newThumbLeft = g.Width - thumb1.Width;

            CmdManager.putCommand(new CameraChangeEndCommand());
        }

        #endregion 拖动事件

        #region 方法

        #region 当前页面自身使用方法

        //初始化方法
        private void InIt()
        {
            myDrawingAttributes.Width = 5;
            myDrawingAttributes.Height = 5;
            inkcanvas.DefaultDrawingAttributes = myDrawingAttributes;
            inkcanvas.EditingMode = InkCanvasEditingMode.None;
            inkcanvas.DefaultDrawingAttributes.FitToCurve = true;
            cmdStack = new CommandStack(inkcanvas.Strokes);
            inkcanvas.Strokes.StrokesChanged += new StrokeCollectionChangedEventHandler(StrokesChangedEvent);
        }

        //平移整个画布
        private void btnTransform_Click(object sender, RoutedEventArgs e)
        {
            foreach (Stroke s in inkcanvas.Strokes)
            {
                Matrix myMatrix = new Matrix();
                myMatrix.Translate(50, 0);
                s.Transform(myMatrix, false);
            }
        }

        //缩放整个画布
        private void btnScale_Click(object sender, RoutedEventArgs e)
        {

            foreach (Stroke s in inkcanvas.Strokes)
            {
                Matrix myMatrix = new Matrix();
                myMatrix.Scale(0.8, 0.8);
                s.Transform(myMatrix, false);
            }
        }

        //清空整个画布
        private void ClearStrokes()
        {
            inkcanvas.Strokes.Clear();
        }

        //通过复制修改画笔颜色
        private DrawingAttributes CloneDrawingAttributes(DrawingAttributes attributes)
        {
            if (attributes == null)
                return attributes;

            DrawingAttributes cloneAttribute = new DrawingAttributes();
            cloneAttribute.Color = attributes.Color;
            cloneAttribute.Height = attributes.Height;
            cloneAttribute.Width = attributes.Width;
            return cloneAttribute;
        }
        //通过复制修改几何图形的颜色
        private SolidColorBrush CloneSolidBrush(SolidColorBrush scb)
        {
            if (scb == null)
                return scb;

            SolidColorBrush cloneScb = new SolidColorBrush();
            cloneScb.Color = scb.Color;
            return cloneScb;
        }


        private void ReviewStroke()
        {
            //回放&时光机
            StrokeCollection strokecollection = new StrokeCollection();
            strokecollection = inkcanvas.Strokes.Clone();
            inkcanvas.Strokes.Clear();
            ThreadPool.QueueUserWorkItem((o) =>
            {
                foreach (Stroke s in strokecollection)
                {
                    Thread.Sleep(100);
                    inkcanvas.Dispatcher.Invoke(new Action(() =>
                    {
                        inkcanvas.Strokes.Add(s);
                    }));
                }
            });

        }

        //保存画板
        public void SaveFile()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Ink Serialized Format (*.isf)|*.isf|" +
                         "Bitmap files (*.bmp)|*.bmp|" + "Bitmap files (*.jpg)|*.jpg";
            dlg.Title = "保存画布";

            dlg.ShowDialog();
            try
            {
                using (FileStream file = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write))
                {
                    Tool.Log("保存" + dlg.FileName);

                    if (dlg.FilterIndex == 1)
                    {
                        AutoSave();
                        inkcanvas.Strokes.Save(file);
                        file.Close();
                    }
                    else
                    {
                        int marg = int.Parse(this.inkcanvas.Margin.Left.ToString());
                        RenderTargetBitmap rtb = new RenderTargetBitmap((int)this.inkcanvas.ActualWidth - marg,
                                        (int)this.inkcanvas.ActualHeight - marg, 0, 0, PixelFormats.Default);
                        rtb.Render(this.inkcanvas);
                        BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(rtb));
                        encoder.Save(file);
                        file.Close();
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, Title);
            }
        }

        #endregion 当前页面自身使用方法

        #region 供指令调用方法

        //画直线方法
        public void DrawLine(Point startPoint, Point endPoint)
        {

            ClearIrregularStroke();
            double scale = thumb1.Width / mobileWidth;
            StylusPointCollection s = new StylusPointCollection();
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + startPoint.X * scale, Canvas.GetTop(thumb1) + startPoint.Y * scale));
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + endPoint.X * scale, Canvas.GetTop(thumb1) + endPoint.Y * scale));
            LineStroke linestroke = new LineStroke(s, CloneDrawingAttributes(myDrawingAttributes));
            inkcanvas.Strokes.Add(linestroke);

        }

        //画椭圆方法
        public void DrawEllipse(Point ra, Point rb, Point rc, Point rd)
        {
            ClearIrregularStroke();
            double scale = thumb1.Width / mobileWidth;
            StylusPointCollection s = new StylusPointCollection();
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + ra.X * scale, Canvas.GetTop(thumb1) + ra.Y * scale));
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + rb.X * scale, Canvas.GetTop(thumb1) + rb.Y * scale));
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + rc.X * scale, Canvas.GetTop(thumb1) + rc.Y * scale));
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + rd.X * scale, Canvas.GetTop(thumb1) + rd.Y * scale));

            EllipseStroke ellipsestroke = new EllipseStroke(s, CloneDrawingAttributes(myDrawingAttributes));
            inkcanvas.Strokes.Add(ellipsestroke);

        }

        //画矩形方法
        public void DrawRectangle(Point ra, Point rb, Point rc, Point rd)
        {
            //ra 是y轴最大 x轴最小的点 rb rc rd 按照逆时针顺序
            ClearIrregularStroke();
            double scale = thumb1.Width / mobileWidth;
            StylusPointCollection s = new StylusPointCollection();
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + ra.X * scale, Canvas.GetTop(thumb1) + ra.Y * scale));
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + rb.X * scale, Canvas.GetTop(thumb1) + rb.Y * scale));
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + rc.X * scale, Canvas.GetTop(thumb1) + rc.Y * scale));
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + rd.X * scale, Canvas.GetTop(thumb1) + rd.Y * scale));
            RectangleStroke rectangleStroke = new RectangleStroke(s, CloneDrawingAttributes(myDrawingAttributes));
            inkcanvas.Strokes.Add(rectangleStroke);

        }

        //画圆方法
        public void DrawCircle(Point centerpoint, double radius)
        {

            ClearIrregularStroke();
            double scale = thumb1.Width / mobileWidth;
            StylusPointCollection s = new StylusPointCollection();
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + centerpoint.X * scale, Canvas.GetTop(thumb1) + centerpoint.Y * scale));
            CircleStroke circlestroke = new CircleStroke(s, CloneDrawingAttributes(myDrawingAttributes));
            circlestroke.radius = radius * scale;
            inkcanvas.Strokes.Add(circlestroke);

        }

        //画三角形方法
        public void DrawTriangle(Point point1, Point point2, Point point3)
        {
            ClearIrregularStroke();
            double scale = thumb1.Width / mobileWidth;
            StylusPointCollection s = new StylusPointCollection();
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + point1.X * scale, Canvas.GetTop(thumb1) + point1.Y * scale));
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + point2.X * scale, Canvas.GetTop(thumb1) + point2.Y * scale));
            s.Add(new StylusPoint(Canvas.GetLeft(thumb1) + point3.X * scale, Canvas.GetTop(thumb1) + point3.Y * scale));
            TriangleStroke trianglestroke = new TriangleStroke(s, CloneDrawingAttributes(myDrawingAttributes));
            inkcanvas.Strokes.Add(trianglestroke);
        }

        //修改画笔
        public void ChangePaint(int color, float size)
        {
            int color_a = (color >> 24) & 0xFF;
            int color_r = (color >> 16) & 0xFF;
            int color_g = (color >> 8) & 0xFF;
            int color_b = color & 0xFF;
            byte a = (byte)color_a;
            byte r = (byte)color_r;
            byte g = (byte)color_g;
            byte b = (byte)color_b;

            this.myColor.A = a;
            this.myColor.B = b;
            this.myColor.G = g;
            this.myColor.R = r;
            mySolidColorBrush.Color = this.myColor;
            //RectangleSetColor.Background = scb;
            //RectangleSetColor.Content = scb.ToString();
            if (inkcanvas != null)
            {
                myDrawingAttributes.Color = this.myColor;
                myDrawingAttributes.Width = size / thumbScaleMultiple;
                myDrawingAttributes.Height = size / thumbScaleMultiple;
                this.inkcanvas.DefaultDrawingAttributes = myDrawingAttributes;
                //textblockOperate.Text = "修改画笔";
            }
        }

        //画笔开始移动
        public void PathStartMove(double x, double y)
        {
            double scale = thumb1.Width / mobileWidth;
            StartUpdateInkcanvas(Canvas.GetLeft(thumb1) + x * scale, Canvas.GetTop(thumb1) + y * scale);
        }

        //画笔正在移动
        public void PathOnMove(double x, double y)
        {
            //坐标转换
            double scale = thumb1.Width / mobileWidth;
            UpdateInkcanvas(Canvas.GetLeft(thumb1) + x * scale, Canvas.GetTop(thumb1) + y * scale);
        }

        //画笔停止移动
        public void PathEndMove()
        {
            myScrollViewer.ScrollToVerticalOffset(newStroke.StylusPoints[newStroke.StylusPoints.Count - 1].Y);
            myScrollViewer.ScrollToHorizontalOffset(newStroke.StylusPoints[newStroke.StylusPoints.Count - 1].X);
        }

        public void SYSN_START()
        {
            IsLixian = true;
            waitingWindow = new NavigationWindow();
            waitingWindow.Source = new Uri("page/WaitingPage.xaml", UriKind.Relative);
            waitingWindow.Title = "正在同步,请稍等";
            waitingWindow.ResizeMode = ResizeMode.NoResize;
            waitingWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            waitingWindow.ShowInTaskbar = true;
            waitingWindow.Width = 525;
            waitingWindow.Height = 400;
            waitingWindow.Show();
        }

        public void SYSN_END()
        {
            IsLixian = false;

            foreach (Stroke s in LixianCollection)
            {
                inkcanvas.Strokes.Add(s);
            }

            CmdManager.putCommand(new SysnEndCommand());
            if (waitingWindow != null)
            {
                waitingWindow.Close();
            }

        }



        //视野开始拖动缩放
        public void CameraStartChange(CONTROL from)
        {
            if (from != CONTROL.SOCKET)
            {
                CmdManager.putCommand(new CameraChangeStartCommand());
            }

            //MessageBox.Show("oldleft: " +oldleft + "oldtop: "+ oldtop);
            thumb1.Background = Brushes.Aqua;
            thumb1.Opacity = 0.5;
        }

        double totalmovex = 0;

        double oldtop = 0;
        double oldleft = 0;

        //视野正在移动缩放
        public void CameraOnChange(float offsetX, float offsetY, float ratio)
        {
            double cameratop = orgin.Top + offsetY / thumbScaleMultiple;
            double cameraleft = orgin.Left + offsetX / thumbScaleMultiple;

            double movetop = (cameratop - oldtop);
            double moveleft = (cameraleft - oldleft);

            totalmovex = totalmovex + moveleft;

            Tool.Log("★CameraOnChange  cameraleft:" + cameraleft + "cameratop:" + cameratop);

            myRatio = ratio;
            Canvas.SetLeft(thumb1, cameraleft);
            Canvas.SetTop(thumb1, cameratop);
            thumb1.Width = orgin.Width * ratio;
            thumb1.Height = orgin.Height * ratio;

            myScrollViewer.ScrollToVerticalOffset(Canvas.GetTop(thumb1));
            myScrollViewer.ScrollToHorizontalOffset(Canvas.GetLeft(thumb1));

            /*
            if (cameratop <= 0)
            {
                Canvas.SetTop(thumb1, 0);
                Tool.Log("★小于0   movetop: " + movetop);


                foreach (Stroke s in inkcanvas.Strokes)
                {
                    Matrix myMatrix = new Matrix();
                    myMatrix.Translate(0, -movetop);
                    s.Transform(myMatrix, false);
                }
            }
            else {
                Canvas.SetLeft(thumb1, cameraleft);
                Canvas.SetTop(thumb1, cameratop);

            }

            if (cameraleft <= 0)
            {
                Canvas.SetLeft(thumb1, 0);
                Tool.Log("★小于0   moveleft: " + moveleft);
                foreach (Stroke s in inkcanvas.Strokes)
                {
                    Matrix myMatrix = new Matrix();
                    myMatrix.Translate(-moveleft, 0);
                    s.Transform(myMatrix, false);
                }
            }
            else {

                Canvas.SetLeft(thumb1, cameraleft);
                Canvas.SetTop(thumb1, cameratop);

            }


            myScrollViewer.ScrollToVerticalOffset(Canvas.GetTop(thumb1));
            myScrollViewer.ScrollToHorizontalOffset(Canvas.GetLeft(thumb1));

            oldleft = Canvas.GetLeft(thumb1);
            oldtop = Canvas.GetTop(thumb1);

            Tool.Log("!!!value == movetop:" + movetop + "moveleft: " + moveleft);
            */

        }

        //视野停止移动缩放
        public void CameraEndChange()
        {
            oldtop = Canvas.GetTop(thumb1);
            oldleft = Canvas.GetLeft(thumb1);

            /*
            double movetop = (Canvas.GetTop(thumb1) - oldtop);
            double moveleft = (Canvas.GetLeft(thumb1) - oldleft);


            if (Canvas.GetTop(thumb1) <= 0)
            {

                Tool.Log("!!!小于0   movetop: " + movetop);
                foreach (Stroke s in inkcanvas.Strokes)
                {
                    Matrix myMatrix = new Matrix();
                    myMatrix.Translate(0, -movetop/2);
                    s.Transform(myMatrix, false);
                }
            }

            if (Canvas.GetLeft(thumb1) <= 0)
            {
                Tool.Log("!!!小于0   moveleft: " + moveleft);
                foreach (Stroke s in inkcanvas.Strokes)
                {
                    Matrix myMatrix = new Matrix();
                    myMatrix.Translate(-moveleft/2, 0);
                    s.Transform(myMatrix, false);
                }
            }


            myScrollViewer.ScrollToVerticalOffset(Canvas.GetTop(thumb1));
            myScrollViewer.ScrollToHorizontalOffset(Canvas.GetLeft(thumb1));
            */
            Tool.Log("total " + totalmovex);
            thumb1.Background = null;
            thumb1.Opacity = 0.2;
        }

        //初始化，手机信息
        public void DEVICE_SYSN(float width, float height, float dpi)
        {
            //未执行

        }

        //初始化矩形框的的位置大小
        public void CAMERA_SYSN(float left, float top, float height, float width, float ratio)
        {
            double thumbleft = 0;
            double thumbtop = 0;
            Tool.Log("CAMERA_SYSN");
            mobileWidth = width;
            myRatio = ratio;
            thumbWidth = width / thumbScaleMultiple / ratio;
            thumbHeight = height / thumbScaleMultiple / ratio;

            thumb1.Width = thumbWidth;
            thumb1.Height = thumbHeight;

            thumbleft = Math.Abs(bggrid.ActualWidth - thumb1.ActualWidth) / 2 / ratio;
            thumbtop = Math.Abs(bggrid.ActualHeight - thumb1.ActualHeight) / 2 / ratio;

            Canvas.SetTop(thumb1, thumbtop);
            Canvas.SetLeft(thumb1, thumbleft);

            orgin = new Rect(thumbleft, thumbtop, thumb1.Width, thumb1.Height);
        }

        //Undo
        public void Undo()
        {
            cmdStack.Undo();
            //Thread threadundo = new Thread(threadUndo);
            //threadundo.Start();

        }
        //Redo
        public void Redo()
        {
            cmdStack.Redo();
            //Thread threadredo = new Thread(threadRedo);
            //threadredo.Start();
        }

        //清空不规则几何图形
        public void ClearIrregularStroke()
        {
            if (IsLixian)
            {
                LixianCollection.RemoveAt(LixianCollection.Count - 1);
            }
            else
            {
                cmdStack.StrokeCollection.RemoveAt(cmdStack.StrokeCollection.Count - 1);
                cmdStack.undoStack.Pop();
                cmdStack.undoStack.Pop();
            }
        }

        //笔迹开始移动
        private void StartUpdateInkcanvas(double x, double y)
        {
            StylusPointCollection stylusPointCollection = new StylusPointCollection();
            StylusPoint stylusPoint = new StylusPoint(x, y);
            stylusPointCollection.Add(stylusPoint);
            newStroke = new Stroke(stylusPointCollection);
            newStroke.DrawingAttributes = CloneDrawingAttributes(myDrawingAttributes);
            //开启线条平滑
            //newStroke.DrawingAttributes.FitToCurve = true;
            if (IsLixian)
            {
                LixianCollection.Add(newStroke);
            }
            else
            {
                inkcanvas.Strokes.Add(newStroke);
            }
        }

        //笔迹正在移动
        private void UpdateInkcanvas(double x, double y)
        {
            StylusPoint newStylusPoint = new StylusPoint(x, y);
            if (newStroke != null)
            {
                newStroke.StylusPoints.Add(newStylusPoint);
            }
        }

        #endregion 供指令调用方法

        #endregion 方法

        private void StrokesChangedEvent(object sender, StrokeCollectionChangedEventArgs e)
        {
            StrokeCollection added = new StrokeCollection(e.Added);
            StrokeCollection removed = new StrokeCollection(e.Removed);
            CommandItem item = new StrokesAddedOrRemovedCI(cmdStack, inkcanvas.EditingMode, added, removed, 0);
            cmdStack.Enqueue(item);
        }


        private void thumb1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Tool.Log("大小改变");
            //MessageBox.Show("大小改变");
            // Tool.Log("thumb大小————> 宽：" +thumb1.ActualWidth+"高：" +thumb1.ActualHeight);
            Tool.Log("!!!thumb1_SizeChanged" + Canvas.GetTop(thumb1) + "   " + double.IsNaN(Canvas.GetLeft(thumb1)));
            
            if (!double.IsNaN(Canvas.GetTop(thumb1)) && !double.IsNaN(Canvas.GetLeft(thumb1)))
            {
                
                myScrollViewer.ScrollToVerticalOffset(Canvas.GetTop(thumb1));
                myScrollViewer.ScrollToHorizontalOffset(Canvas.GetLeft(thumb1));
            }
        }

        private void btnHidden_Click(object sender, RoutedEventArgs e)
        {
            if (!IsHidden)
            {
                imageMenu.Visibility = Visibility.Hidden;
                btnReview.Visibility = Visibility.Hidden;
                btnSave.Visibility = Visibility.Hidden;
                IsHidden = true;

                imgeye.Source = new BitmapImage(new Uri(@"/SyncPaint;component/picture/眼睛.png", UriKind.Relative));
            }
            else
            {
                imageMenu.Visibility = Visibility.Visible;
                btnReview.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Visible;
                IsHidden = false;
                imgeye.Source = new BitmapImage(new Uri(@"/SyncPaint;component/picture/隐藏.png", UriKind.Relative));
            }
        }


        private void Close(object sender, RoutedEventArgs e)
        {
            CmdManager.closrListener();
            if (CmdManager.main == null)
            {
                MainWindow main = new MainWindow();
                main.Show();
            }
            else
            {
                CmdManager.main.Visibility = Visibility.Visible;
            }
            AutoSave();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            CmdManager.DrawPage = this;
        }

        private void btntest_Click(object sender, RoutedEventArgs e)
        {
            foreach (Stroke s in inkcanvas.Strokes)
            {
                Matrix myMatrix = new Matrix();
                myMatrix.Translate(thumb1.ActualWidth, 0);
                s.Transform(myMatrix, false);
            }
        }

        private void AutoSave() {
            if (inkcanvas.Strokes.Count != 0)
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                string path = Environment.CurrentDirectory + "\\strokeSave\\" + Convert.ToInt64(ts.TotalSeconds).ToString() + ".isf";
                FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
                inkcanvas.Strokes.Save(file);
            }

        }

        private void btntestsave_Click(object sender, RoutedEventArgs e)
        {
            AutoSave();
        }
    }
}
