using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncPaint.command
{

    class PathMoveOnCommand : Command
    {
        private static String format = "{{type:{0},X:{1},Y{2} }}";

        public float X { get; private set; }

        public float Y { get; private set; }

        public PathMoveOnCommand(float x,float y) : base(CommandType.PATH_MOVE_ON)
        {
            X = x;
            Y = y;
        }

        public PathMoveOnCommand(byte[]data) : base(CommandType.PATH_MOVE_ON)
        {
            init(data);
        }


        public override void init(byte[] data)
        {
            X = ConvertData.bytesToFloat(data, 0);
            Y = ConvertData.bytesToFloat(data, 4);
        }

        public override byte[] toBytes()
        {
            byte[] data = new byte[Length];
            data[0] = CommandMap.getByteFromCommandType(CommandType.PATH_MOVE_ON);
            ConvertData.FloatTobytes(data, 1, X);
            ConvertData.FloatTobytes(data, 5, Y);
            return data;
        }

        public override string ToString()
        {
            return String.Format(format, Type.ToString(),X,Y);
        }
    }
}
