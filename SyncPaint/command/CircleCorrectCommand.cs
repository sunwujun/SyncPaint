using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
namespace SyncPaint.command
{
    class CircleCorrectCommand : ShapeCorrectCommand
    {
        public Point center;
        public float radius;
        public CircleCorrectCommand(byte[]data):base(ShapeType.CIRCLE)
        {
            center = new Point();

            center.X = ConvertData.bytesToFloat(data,0);
            center.Y = ConvertData.bytesToFloat(data, 4);

            radius = ConvertData.bytesToFloat(data,8);
        }
        public override void init(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] toBytes()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "形状修正为圆 中心 " + center.ToString() + " 半径 " + radius;
        }
    }
}
