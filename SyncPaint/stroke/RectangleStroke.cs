using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace SyncPaint.stroke
{
    //自定义Stroke 绘制矩形
    class RectangleStroke : Stroke
    {
        public RectangleStroke(StylusPointCollection stylusPointCollection, DrawingAttributes drawingattributes)
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

            RotateTransform rotatetransform = new RotateTransform();
            rotatetransform.CenterX = (StylusPoints[0].X + StylusPoints[1].X) / 2;
            rotatetransform.CenterX = (StylusPoints[0].Y + StylusPoints[1].Y) / 2;
            Transform transform = rotatetransform;
            drawingContext.PushTransform(transform);

            LineGeometry linegeometry1 = new LineGeometry(new Point(StylusPoints[0].X, StylusPoints[0].Y), new Point(StylusPoints[1].X, StylusPoints[1].Y));
            LineGeometry linegeometry2 = new LineGeometry(new Point(StylusPoints[1].X, StylusPoints[1].Y), new Point(StylusPoints[2].X, StylusPoints[2].Y));
            LineGeometry linegeometry3 = new LineGeometry(new Point(StylusPoints[2].X, StylusPoints[2].Y), new Point(StylusPoints[3].X, StylusPoints[3].Y));
            LineGeometry linegeometry4 = new LineGeometry(new Point(StylusPoints[3].X, StylusPoints[3].Y), new Point(StylusPoints[0].X, StylusPoints[0].Y));
            GeometryGroup geometrygroup = new GeometryGroup();
            geometrygroup.Children.Add(linegeometry1);
            geometrygroup.Children.Add(linegeometry2);
            geometrygroup.Children.Add(linegeometry3);
            geometrygroup.Children.Add(linegeometry4);
            drawingContext.DrawGeometry(null, pen, geometrygroup);
        }
    }
}
