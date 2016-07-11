using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace SyncPaint.command
{
    class CommandMap
    {
        class bean
        {

            public byte TypeByte { get; set; }


            public CommandType Type { get; set; }

            public int Length { get; set; }

            public bean(int length, CommandType type, byte typebyte)
            {
                this.TypeByte = typebyte;
                this.Length = length;
                this.Type = type;
            }
        }

        private const int COMMANDNUMBER = 11;


        private static Hashtable byteintmap = new Hashtable();

        private static Hashtable typeintmap = new Hashtable();

        static CommandMap()
        {
            byteintmap.Add(0x1, 0);
            byteintmap.Add(0x2, 1);
            byteintmap.Add(0x3, 2);
            byteintmap.Add(0x4, 3);
            byteintmap.Add(0x5, 4);
            byteintmap.Add(0x6, 5);
            byteintmap.Add(0x7, 6);
            byteintmap.Add(0x8, 7);
            byteintmap.Add(0x9, 8);
            byteintmap.Add(0xa, 9);
            byteintmap.Add(0xb, 10);
            byteintmap.Add(0xc, 11);
            byteintmap.Add(0xd, 12);

            byteintmap.Add(0xf, 14);


            typeintmap.Add(CommandType.PATH_MOVE_START, 0);
            typeintmap.Add(CommandType.PATH_MOVE_ON, 1);
            typeintmap.Add(CommandType.PATH_MOVE_END, 2);
            typeintmap.Add(CommandType.CAMERA_CHANGE_START, 3);
            typeintmap.Add(CommandType.CAMERA_CHANGE_ON, 4);
            typeintmap.Add(CommandType.CAMERA_CHANGE_END, 5);
            typeintmap.Add(CommandType.SYSN_START, 6);
            typeintmap.Add(CommandType.DEVICE_SYSN, 7);
            typeintmap.Add(CommandType.CAMERA_SYSN, 8);
            typeintmap.Add(CommandType.SYSN_END, 9);
            typeintmap.Add(CommandType.PAINT_CHANGE, 10);
            typeintmap.Add(CommandType.UNDO, 11);
            typeintmap.Add(CommandType.REDO, 12); 
            typeintmap.Add(CommandType.NULL, 12);
            typeintmap.Add(CommandType.SHAPECORRECT, 14);


        }

        private static bean[] map = {
            new bean(9, CommandType.PATH_MOVE_START, (byte) 0x1),
            new bean(9, CommandType.PATH_MOVE_ON, (byte) 0x2),
            new bean(1, CommandType.PATH_MOVE_END, (byte) 0x3),
            new bean(1, CommandType.CAMERA_CHANGE_START, (byte) 0x4),
            new bean(13, CommandType.CAMERA_CHANGE_ON, (byte) 0x5),
            new bean(1, CommandType.CAMERA_CHANGE_END, (byte) 0x6),
            new bean(1, CommandType.SYSN_START, (byte) 0x7),
            new bean(13, CommandType.DEVICE_SYSN, (byte) 0x8),
            new bean(21, CommandType.CAMERA_SYSN, (byte) 0x9),
            new bean(1, CommandType.SYSN_END, (byte) 0xa),
            new bean(13, CommandType.PAINT_CHANGE, (byte) 0xb),
            new bean(1, CommandType.UNDO, (byte) 0xc),
            new bean(1, CommandType.REDO, (byte) 0xd),
            new bean(1,CommandType.NULL, (byte) 0xe),
            new bean(1,CommandType.SHAPECORRECT, (byte) 0xf),

    };

        public static CommandType getCommandTypeFromByte(byte data)
        {
            int index = 0;

            index = (int)byteintmap[(int)data];

        
            return map[index].Type;
        }

        private void jumpToErcode() {
            /*
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, delegate ()
            {
                NavigationWindow window = new NavigationWindow();
                window.Source = new Uri("page/ErcodePage.xaml", UriKind.Relative);
                window.Title = "手机扫码";
                window.ResizeMode = ResizeMode.NoResize;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.ShowInTaskbar = true;
                window.Width = 600;
                window.Height = 500;
                window.Show();

            });*/
        }


        public static byte getByteFromCommandType(CommandType type)
        {
            return map[(int)typeintmap[type]].TypeByte;
        }


        public static int getTypeLength(CommandType type)
        {
            return map[(int)typeintmap[type]].Length;
        }
    }
}
