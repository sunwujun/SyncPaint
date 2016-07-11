using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncPaint.command
{
    class PathMoveEndCommand : Command
    {
        private static  String format="{{type:{0}}}";
        public PathMoveEndCommand(byte[]data):base(CommandType.PATH_MOVE_END){

        }


        public override void init(byte[] data)
        {
            
        }

        public override byte[] toBytes()
        {
            byte[] data = new byte[1];
            data[0] = CommandMap.getByteFromCommandType(CommandType.PATH_MOVE_END);
            return data;

        }

        public override string ToString()
        {
            return String.Format(format, Type.ToString());
        }
    }
}
