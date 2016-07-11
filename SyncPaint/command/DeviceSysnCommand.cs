using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncPaint.command
{
    class DeviceSysnCommand : Command
    {

        public float Width { get; private set; }
        public float Height { get; private set; }
        public float Dpi { get; private set; }

        private static String format = "{{type:{0},Width:{1},Height:{2},Dpi:{3}}}";

        public DeviceSysnCommand(byte[] data) : base(CommandType.DEVICE_SYSN)
        {
            init(data);
        }
        public override void init(byte[] data)
        {
            Width = ConvertData.bytesToFloat(data, 0);
            Height = ConvertData.bytesToFloat(data, 4);
            Dpi = ConvertData.bytesToFloat(data, 8);

        }

        public override byte[] toBytes()
        {
            byte[] data = new byte[Length];
            data[0] = CommandMap.getByteFromCommandType(Type);
            ConvertData.FloatTobytes(data, 1, Width);
            ConvertData.FloatTobytes(data, 5, Height);
            ConvertData.FloatTobytes(data, 9, Dpi);
            return data;
        }

        public override string ToString()
        {
            return String.Format(format,Type.ToString(),Width,Height,Dpi);
          
        }
    }
}
