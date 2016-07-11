using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace SyncPaint.stroke
{
    //自定义Stroke 绘制圆形
    class CircleStroke : Stroke
    {
        public double radius;
        public CircleStroke(StylusPointCollection stylusPointCollection, DrawingAttributes drawingattributes)
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
            Point centerpoint = new Point(StylusPoints[0].X, StylusPoints[0].Y);
            drawingContext.DrawEllipse(null, pen, centerpoint, radius, radius);
        }
    }
}
