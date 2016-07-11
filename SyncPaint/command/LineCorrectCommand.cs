using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SyncPaint.command
{
    class LineCorrectCommand : ShapeCorrectCommand
    {

        public Point start, end;
        public LineCorrectCommand(byte[]data):base(ShapeType.LINE)
        {
            
            start.X = (double)ConvertData.bytesToFloat(data,0);
            start.Y = (double)ConvertData.bytesToFloat(data, 4);
            end.X = (double)ConvertData.bytesToFloat(data, 8);
            end.Y = (double)ConvertData.bytesToFloat(data, 12);
        }
        public override void init(byte[] data)
        {
        }

        public override byte[] toBytes()
        {
            return null;
        }

        public override string ToString()
        {
            return "形状修正 " + start.ToString() + " " + end.ToString();
        }
    }
}
