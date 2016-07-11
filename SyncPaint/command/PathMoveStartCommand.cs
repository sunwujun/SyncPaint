using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncPaint.command
{
    class PathMoveStartCommand : Command
    {
        public float X { get; private set; }
    
        public float Y { get; private set; }

        private  const string format= "{{type:{0},X:{1},Y:{2}}}";

        public PathMoveStartCommand(byte[] data) : base(CommandType.PATH_MOVE_START)
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
            data[0] = CommandMap.getByteFromCommandType(Type);
            ConvertData.FloatTobytes(data, 1, X);
            ConvertData.FloatTobytes(data, 5, Y);
            return data;
        }

        public override string ToString()
        {
            return String.Format(format,Type.ToString(),X,Y);
        }
    }
}
