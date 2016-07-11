using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncPaint.command
{
    abstract class Command
    {
        //每个命令都有自己的类型
        private CommandType type;

        public CommandType Type { get; private set; }

        public int Length { get; private set; }

        public Command(CommandType type)
        {
            Type = type;
            Length = CommandMap.getTypeLength(Type);
        }

        public Command(byte[] data) {
            init(data);
        }

        public abstract void init(byte[] data);


        public abstract byte[] toBytes();

        public override abstract string ToString();
    }
}
