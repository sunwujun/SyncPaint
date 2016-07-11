using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncPaint.command
{
    class CameraSysnCommand : Command
    {

        public float Left { get; private set; }
        public float Top { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float Ratio { get; private set; }
        

        private const String format= "{{type:{0},left:{1},top:{2},width:{3},height:{4},ratio:{5}}}";
   
        public CameraSysnCommand(byte[] data):base(CommandType.CAMERA_SYSN)
        {
            init(data);

        }
        public override void init(byte[] data)
        {
            Left = ConvertData.bytesToFloat(data, 0);
            Top = ConvertData.bytesToFloat(data, 4);
            Width = ConvertData.bytesToFloat(data, 8);
            Height = ConvertData.bytesToFloat(data, 12);
            Ratio = ConvertData.bytesToFloat(data, 16);
        }

        public override byte[] toBytes()
        {
            byte[] data = new byte[Length];
            data[0] = CommandMap.getByteFromCommandType(Type);
            ConvertData.FloatTobytes(data, 1, Left);
            ConvertData.FloatTobytes(data, 5, Top);
            ConvertData.FloatTobytes(data, 9, Width);
            ConvertData.FloatTobytes(data, 13, Height);
            ConvertData.FloatTobytes(data, 17, Ratio);
            return data;
        }

        public override string ToString()
        {
            return String.Format(format, Type.ToString(), Left, Top, Width, Height, Ratio);

        }
    }
}
