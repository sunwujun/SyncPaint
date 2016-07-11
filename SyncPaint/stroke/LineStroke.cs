using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace SyncPaint.stroke
{

    //自定义Stroke 绘制直线
    class LineStroke : Stroke
    {
        public LineStroke(StylusPointCollection stylusPointCollection, DrawingAttributes drawingattributes)
           : base(stylusPointCollection)
        {
            this.StylusPoints = stylusPointCollection;
            this.DrawingAttributes = drawingattributes;
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
            drawingContext.DrawLine(pen, new Point(StylusPoints[0].X, StylusPoints[0].Y),
                    new Point(StylusPoints[1].X, StylusPoints[1].Y));
        }
    }
}
