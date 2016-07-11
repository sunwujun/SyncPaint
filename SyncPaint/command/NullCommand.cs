using SyncPaint.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncPaint.command
{
    class NullCommand : Command
    {

        public NullCommand() : base(CommandType.NULL) { }

        public NullCommand(byte[] data) : base(CommandType.NULL) { init(data); }

        public override void init(byte[] data)
        {
        }

        public override byte[] toBytes()
        {
            byte[] data = new byte[1];
            data[0] = CommandMap.getByteFromCommandType(CommandType.NULL);
            return data;
        }

        public override string ToString()
        {
            return "something wrong " + Type.ToString();
        }
    }
}
