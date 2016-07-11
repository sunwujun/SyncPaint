using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
namespace SyncPaint.command
{
    class TriangleCorrectCommand : ShapeCorrectCommand
    {
        public Point ta, tb, tc;
        public TriangleCorrectCommand(byte[]data):base(ShapeType.TRIANGLE)
        {
            ta = new Point();
            tb = new Point();
            tc = new Point();
            ta.X = ConvertData.bytesToFloat(data, 0);
            ta.Y = ConvertData.bytesToFloat(data, 4);

            tb.X = ConvertData.bytesToFloat(data, 8);
            tb.Y = ConvertData.bytesToFloat(data, 12);

            tc.X = ConvertData.bytesToFloat(data, 16);
            tc.Y = ConvertData.bytesToFloat(data, 20);

        }
        public override void init(byte[] data)
        {
            
        }

        public override byte[] toBytes()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
           return "形状修正为三角形 " + ta.ToString() + " " + tb.ToString() + " " + tc.ToString();
        }
    }
}
