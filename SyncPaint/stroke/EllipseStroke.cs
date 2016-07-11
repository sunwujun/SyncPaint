using System;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace SyncPaint.stroke
{
    //自定义Stroke 绘制椭圆
    class EllipseStroke : Stroke
    {
        public double angle;
        public EllipseStroke(StylusPointCollection stylusPointCollection, DrawingAttributes drawingattributes)
            : base(stylusPointCollection)
        {
            StylusPoints = stylusPointCollection;
            DrawingAttributes = drawingattributes;
        }

        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {

            Pen pen = new Pen
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                Brush = new SolidColorBrush(drawingAttributes.Color),
                Thickness = drawingAttributes.Width
            };
            Point centerpoint = new Point((StylusPoints[0].X + StylusPoints[2].X) / 2, (StylusPoints[0].Y + StylusPoints[2].Y) / 2);
            angle = Math.Atan((StylusPoints[1].Y - StylusPoints[0].Y) / (StylusPoints[1].X - StylusPoints[0].X));
            angle = 180 * angle / Math.PI;
            RotateTransform rotatetransform = new RotateTransform();
            rotatetransform.CenterX = centerpoint.X;
            rotatetransform.CenterY = centerpoint.Y;
            rotatetransform.Angle = angle;
            Transform transform = rotatetransform;
            drawingContext.PushTransform(transform);

            Math.Pow(StylusPoints[0].X - StylusPoints[1].X, 2);
            Math.Pow(StylusPoints[0].Y - StylusPoints[1].Y, 2);
            double radiusX = Math.Sqrt(Math.Pow(StylusPoints[0].X - StylusPoints[1].X, 2) + Math.Pow(StylusPoints[0].Y - StylusPoints[1].Y, 2)) / 2;
            double radiusY = Math.Sqrt(Math.Pow(StylusPoints[1].X - StylusPoints[2].X, 2) + Math.Pow(StylusPoints[1].Y - StylusPoints[2].Y, 2)) / 2;
            drawingContext.DrawEllipse(null, pen, centerpoint, radiusX, radiusY);

        }
    }
}
