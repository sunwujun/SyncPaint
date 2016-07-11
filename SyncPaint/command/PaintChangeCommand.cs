using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncPaint.command
{
    class PaintChangeCommand : Command
    {

        public int Color { get; private set; }
        public float Size { get; private set; }
        public float Blur { get; private set; }

        public const string format = "{{type:{0},color:{1},size:{2},blur:{3}}}";

        public PaintChangeCommand(byte[] data) : base(CommandType.PAINT_CHANGE)
        {
            init(data);

        }

        public override void init(byte[] data)
        {
            Color = ConvertData.bytesToInt(data, 0);
            Size = ConvertData.bytesToFloat(data, 4);
            Blur = ConvertData.bytesToFloat(data, 8);
        }

        public override byte[] toBytes()
        {
            byte[] data = new byte[Length];
            data[0] = CommandMap.getByteFromCommandType(Type);
            ConvertData.IntTobytes(data, 1, Color);
            ConvertData.FloatTobytes(data, 5, Size);
            ConvertData.FloatTobytes(data, 9, Blur);
            return data;
        }

        public override string ToString()
        {
            return String.Format(format, Type.ToString(), Color, Size, Blur);
        }
    }
}
