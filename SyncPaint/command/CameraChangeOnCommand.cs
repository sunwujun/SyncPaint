using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SyncPaint.command
{
    class CameraChangeOnCommand : Command
    {

        public float OffsetX { get; private set; }
        public float OffsetY { get; private set; }

        public float Ratio { get; private set; }

        private const String format = "{{  type:CAMERA_CHANGE_ON,OffsetX:{0},OffsetY:{1},Ratio:{2} }}";
        

        public CameraChangeOnCommand(float dx, float dy,  float ratio) : base(CommandType.CAMERA_CHANGE_ON)
        {
            OffsetX = dx;
            OffsetY = dy;
            Ratio = ratio;
        }

        public CameraChangeOnCommand(byte[] data) : base(CommandType.CAMERA_CHANGE_ON)
        {
            init(data);
        }

        public override void init(byte[] data)
        {
            OffsetX = ConvertData.bytesToFloat(data, 0);
            OffsetY = ConvertData.bytesToFloat(data, 4);
            Ratio = ConvertData.bytesToFloat(data, 8);


        }

        public override byte[] toBytes()
        {
            byte[] data = new byte[Length];
            data[0] = CommandMap.getByteFromCommandType(CommandType.CAMERA_CHANGE_ON);
            ConvertData.FloatTobytes(data, 1, OffsetX);
            ConvertData.FloatTobytes(data, 5, OffsetY);
            ConvertData.FloatTobytes(data, 9, Ratio);
            return data;
        }

        public override string ToString()
        {
            return String.Format(format, OffsetX, OffsetY, Ratio);
        }
    }
}
