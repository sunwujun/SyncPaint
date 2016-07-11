using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncPaint.command
{
    abstract class ShapeCorrectCommand:Command
    {
        public ShapeType ShapeType
        {
            get;
            private set;
        }
        public ShapeCorrectCommand(ShapeType type):base(CommandType.SHAPECORRECT)
        {
            ShapeType = type;
        }
    }
}
