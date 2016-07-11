using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
namespace SyncPaint.command
{
    class RectangleCorrectCommand : ShapeCorrectCommand
    {
        public Point lt, rt, rb, lb;
        public RectangleCorrectCommand(byte[]data):base(ShapeType.RECTANGLE)
        {
            lt = new Point();
            rt = new Point();
            rb = new Point();
            lb = new Point();


            lt.X = ConvertData.bytesToFloat(data,0);
            lt.Y = ConvertData.bytesToFloat(data, 4);

            rt.X = ConvertData.bytesToFloat(data, 8);
            rt.Y = ConvertData.bytesToFloat(data, 12);

            rb.X = ConvertData.bytesToFloat(data, 16);
            rb.Y = ConvertData.bytesToFloat(data, 20);

            lb.X = ConvertData.bytesToFloat(data, 24);
            lb.Y = ConvertData.bytesToFloat(data, 28);
            
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
            return "形状修正为 矩形 " + lt.ToString() + " " + rt.ToString() + " " + rb.ToString() + " " + lb.ToString();

        }
    }
}
