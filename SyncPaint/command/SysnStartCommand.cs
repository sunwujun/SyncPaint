using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncPaint.command
{
    class SysnStartCommand : Command
    {

        private static String format = "{{type:{0}}}";
        public SysnStartCommand(byte[] data):base(CommandType.SYSN_START){

        }


        public override void init(byte[] data)
        {

        }

        public override byte[] toBytes()
        {
            byte[] data = new byte[1];
            data[0] = CommandMap.getByteFromCommandType(Type);
            return data;

        }

        public override string ToString()
        {
            return String.Format(format, Type.ToString());
        }
    }
}
