using System;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using SyncPaint.command;
using System.Windows;
using System.Collections;
using System.Windows.Navigation;

namespace SyncPaint
{
   public  enum CONTROL
    {
        SOCKET,
        MOUSE,
    }

    class CmdManager
    {
        static string ip;
        public static TcpListener listener;
        public static TcpClient tcpClient;
        public static bool isConnect = false;
        static DrawPage drawpage;
        static ErcodePage ercodepage;

        public static MainWindow main;

        public static Thread getThread = new Thread(get);
        public static Thread sendThread = new Thread(send);

        public static Hashtable Rcvbuffer = new Hashtable();
        public static byte[] typebyte = new byte[1];

        static Queue cmdQueue = Queue.Synchronized(new Queue());

        static CmdManager()
        {
            Rcvbuffer.Add(1, new byte[0]);
            Rcvbuffer.Add(9, new byte[8]);
            Rcvbuffer.Add(13, new byte[12]);
            Rcvbuffer.Add(21, new byte[20]);
        }

        public static string IP
        {
            get
            {
                return ip;
            }
            set
            {
                ip = value;
            }
        }
        public static DrawPage DrawPage
        {
            set
            {
                drawpage = value;
            }
        }
        public static ErcodePage Ercode
        {
            set
            {
                ercodepage = value;
            }

        }
        public static void setDraw(DrawPage draw)
        {
            drawpage = draw;
        }

        public static void socketStart()
        {
            //等待连接 并交给 send get
            Thread socketthread = new Thread(connect);
            socketthread.Start();
        }



        //从socket中接收并分发命令
        private static void get()
        {
            NetworkStream netStream = null;
            try
            {
                netStream = tcpClient.GetStream();
            }
            catch(System.NullReferenceException) {

            }
            while (pullflag)
            {
                try
                {
                    CommandType type = CommandType.NULL;
                    byte b = (byte)netStream.ReadByte();
                    if (b == -1)
                    {
                        Tool.Log("到流的末尾");
                    }
                    type = CommandMap.getCommandTypeFromByte(b);
                    if (!isConnect && type.Equals(CommandType.SYSN_START))
                    {
                        ercodepage.Dispatcher.Invoke(new Action(() =>
                        {
                            ercodepage.changePage();
                        }));
                        isConnect = true;
                      
                        //发来的是Sysn命令 是第一次连接 应该发会 表示连接成功的命令通知手机
                        while (drawpage == null) { }
                    }

                    int length = CommandMap.getTypeLength(type);
                    byte[] data = null;

                    if (type == CommandType.SHAPECORRECT)
                    {
                        //drawpage.clearirregularstroke();
                        byte shapetype = (byte)netStream.ReadByte();
                        switch (shapetype)
                        {
                            case 0x1://line
                                data = new byte[4 * 2 * 2];//两个点          
                                break;
                            case 0x2://TRIANGLE
                                data = new byte[4 * 2 * 3];//三个点
                                break;
                            case 0x3://RECTANGLE
                                data = new byte[4 * 2 * 4];//四个点
                                break;
                            case 0x4://ELLIPSES
                                data = new byte[4 * 2 * 4];//四个点
                                break;
                            case 0x5://CIRCLE
                                data = new byte[4 * 2 + 4];//两个点+一个float
                                break;
                        }
                        int count = netStream.Read(data, 0, data.Length);
                        ShapeCorrectCommand scmd = null;
                        switch (shapetype)
                        {
                            case 0x1:
                                scmd = new LineCorrectCommand(data);
                                // drawpage.DrawLine(lcmd.start, lcmd.end);
                                //  Tool.Log(lcmd.ToString());

                                break;
                            case 0x2:
                                scmd = new TriangleCorrectCommand(data);

                                //  Tool.Log(tcmd.ToString());

                                break;
                            case 0x3:
                                scmd = new RectangleCorrectCommand(data);

                                // Tool.Log(rcmd.ToString());

                                break;
                            case 0x4:
                                scmd = new EllipseCorrectCommand(data);
                                // Tool.Log(ecmd.ToString());


                                break;
                            case 0x5:
                                scmd = new CircleCorrectCommand(data);
                                //  Tool.Log(ccmd.ToString());
                                break;

                        }
                        if (drawpage != null)
                        {
                            drawpage.Dispatcher.Invoke(new Action(() =>
                            {
                                switch (shapetype)
                                {
                                    case 0x1:
                                        LineCorrectCommand lcmd = (LineCorrectCommand)scmd;
                                        drawpage.DrawLine(lcmd.start, lcmd.end);
                                    // MessageBox.Show("DrawLine");
                                    break;
                                    case 0x2:
                                        TriangleCorrectCommand tcmd = (TriangleCorrectCommand)scmd;
                                        drawpage.DrawTriangle(tcmd.ta, tcmd.tb, tcmd.tc);
                                    //MessageBox.Show("DrawTriangle");
                                    break;
                                    case 0x3:
                                        RectangleCorrectCommand rcmd = (RectangleCorrectCommand)scmd;
                                        drawpage.DrawRectangle(rcmd.rb, rcmd.lb, rcmd.lt, rcmd.rt);

                                    //MessageBox.Show("DrawRectangle");
                                    break;
                                    case 0x4:
                                        EllipseCorrectCommand ecmd = (EllipseCorrectCommand)scmd;
                                        drawpage.DrawEllipse(ecmd.rb, ecmd.lb, ecmd.lt, ecmd.rt);
                                    //drawpage.DrawRectangle(ecmd.rb, ecmd.lb, ecmd.lt, ecmd.rt);

                                    //MessageBox.Show("DrawEllipse");
                                    break;
                                    case 0x5:
                                        CircleCorrectCommand ccmd = (CircleCorrectCommand)scmd;

                                        drawpage.DrawCircle(ccmd.center, ccmd.radius);
                                    //MessageBox.Show("DrawCircle");
                                    break;
                                }
                            }));
                        }

                        continue;

                    }

                    if (length != 1)
                    {
                        data = (byte[])Rcvbuffer[length];
                        int readcount = netStream.Read(data, 0, data.Length);
                    }

                    Command cmd = null;

                    switch (type)
                    {
                        case CommandType.PATH_MOVE_START:
                            cmd = new PathMoveStartCommand(data);
                            break;
                        case CommandType.PATH_MOVE_ON:
                            cmd = new PathMoveOnCommand(data);
                            break;

                        case CommandType.PATH_MOVE_END:
                            cmd = new PathMoveEndCommand(data);

                            break;

                        case CommandType.CAMERA_CHANGE_START:
                            cmd = new CameraChangeStartCommand(data);
                            break;

                        case CommandType.CAMERA_CHANGE_ON:
                            cmd = new CameraChangeOnCommand(data);
                            break;

                        case CommandType.CAMERA_CHANGE_END:
                            cmd = new CameraChangeEndCommand(data);

                            break;

                        case CommandType.SYSN_START:
                            cmd = new SysnStartCommand(data);
                            break;

                        case CommandType.DEVICE_SYSN:
                            cmd = new DeviceSysnCommand(data);
                            break;

                        case CommandType.CAMERA_SYSN:
                            cmd = new CameraSysnCommand(data);
                            break;

                        case CommandType.SYSN_END:
                            cmd = new SysnEndCommand(data);
                            break;

                        case CommandType.PAINT_CHANGE:
                            cmd = new PaintChangeCommand(data);


                            break;

                        case CommandType.UNDO:
                            cmd = new UndoCommand(data);
                            break;

                        case CommandType.REDO:
                            cmd = new RedoCommand(data);
                            break;

                        case CommandType.NULL:
                            cmd = new NullCommand(data);
                            break;

                    }
                    Tool.Log("接收到 " + cmd.ToString());

                    if (drawpage != null)
                    {
                        drawpage.Dispatcher.Invoke(new Action(() =>
                        {
                            switch (cmd.Type)
                            {
                                case CommandType.PATH_MOVE_START:
                                    PathMoveStartCommand cmd0 = (PathMoveStartCommand)cmd;
                                    drawpage.PathStartMove(cmd0.X, cmd0.Y);

                                    break;

                                case CommandType.PATH_MOVE_ON:
                                    PathMoveOnCommand cmd1 = (PathMoveOnCommand)cmd;

                                    drawpage.PathOnMove(cmd1.X, cmd1.Y);
                                    break;

                                case CommandType.PATH_MOVE_END:
                                    PathMoveEndCommand cmd2 = (PathMoveEndCommand)cmd;

                                    drawpage.PathEndMove();
                                    break;

                                case CommandType.CAMERA_CHANGE_START:
                                    CameraChangeStartCommand cmd3 = (CameraChangeStartCommand)cmd;
                                    drawpage.CameraStartChange(CONTROL.SOCKET);
                                    break;

                                case CommandType.CAMERA_CHANGE_ON:
                                    CameraChangeOnCommand cmd4 = (CameraChangeOnCommand)cmd;

                                    drawpage.CameraOnChange(cmd4.OffsetX, cmd4.OffsetY, cmd4.Ratio);

                                    break;

                                case CommandType.CAMERA_CHANGE_END:
                                    CameraChangeEndCommand cmd5 = (CameraChangeEndCommand)cmd;
                                    drawpage.CameraEndChange();

                                    break;

                                case CommandType.SYSN_START:
                                    SysnStartCommand cmd6 = (SysnStartCommand)cmd;
                                    drawpage.SYSN_START();

                                    break;

                                case CommandType.DEVICE_SYSN:
                                    DeviceSysnCommand cmd7 = (DeviceSysnCommand)cmd;

                                    break;

                                case CommandType.CAMERA_SYSN:
                                    CameraSysnCommand cmd8 = (CameraSysnCommand)cmd;
                                    drawpage.CAMERA_SYSN(cmd8.Left, cmd8.Top, cmd8.Height, cmd8.Width, cmd8.Ratio);
                                    break;

                                case CommandType.SYSN_END:
                                    SysnEndCommand cmd9 = (SysnEndCommand)cmd;
                                    drawpage.SYSN_END();
                                    break;

                                case CommandType.PAINT_CHANGE:
                                    PaintChangeCommand cmd10 = (PaintChangeCommand)cmd;
                                    drawpage.ChangePaint(cmd10.Color, cmd10.Size);

                                    break;

                                case CommandType.UNDO:
                                    UndoCommand cmd11 = (UndoCommand)cmd;
                                    drawpage.Undo();
                                    break;

                                case CommandType.REDO:
                                    RedoCommand cmd12 = (RedoCommand)cmd;
                                    drawpage.Redo();
                                    break;

                                case CommandType.NULL:
                                    NullCommand cmd13 = (NullCommand)cmd;
                                    break;

                            }

                        }));
                    }
                }catch(Exception e)
                {
                    Tool.Log("发生异常 "+e.ToString());
                 
                    dealConnectWrong();
                    break;
                }
            }
        }

        //向socket另一端发送命令
        private static void send()
        {
            NetworkStream netStream = null;
            try
            {
                netStream = tcpClient.GetStream();
            }
            catch (System.NullReferenceException) { }
            while (pushflag)
            {
                while (cmdQueue.Count != 0)
                {
                    byte[] cmd = (byte[])cmdQueue.Dequeue();
                    try
                    {
                        netStream.Write(cmd, 0, cmd.Length);
                    }
                    catch(Exception e)
                    {
                        Tool.Log("send 异常");
                        dealConnectWrong();
                        break;
                    }
                }
            }

        }

        public static void dealConnectWrong()
        {
            if (tcpClient != null)
            {
            tcpClient.Close();
            listener.Stop();
            }
     


            listener = null;
            tcpClient = null;
            isConnect = false;
            pullflag = false;
            pushflag = false;


            if (drawpage != null)
            {
                drawpage.Dispatcher.Invoke(new Action(()=> {
                    Window win = (Window)drawpage.Parent;
                    win.Close();
                    drawpage = null;

                    main = new MainWindow();
                    main.Visibility = Visibility.Visible;
                    
                }));

            }

            

        }

       
        static bool pullflag = false;
        static bool pushflag = false;

        public static void connect()
        {
            IPEndPoint localep = new IPEndPoint(IPAddress.Parse(ip), 4567);//本机ip的4567端口
            listener = new TcpListener(localep);
            

            try
            {
                listener.Start(1);//只能接受一个连接

            }
            catch (System.Net.Sockets.SocketException) {
                //IPEndPoint localep1 = new IPEndPoint(IPAddress.Parse(ip), 4568);//本机ip的4567端口
                listener = new TcpListener(localep);
                listener.Start(1);

            }


            try
            {
                tcpClient = listener.AcceptTcpClient();
            }
            catch(System.Net.Sockets.SocketException) {

            }
            pullflag = true;
            pushflag = true;

            new Thread(get).Start();
            new Thread(send).Start();


        }

        public static void putCommand(Command cmd)
        {
            Tool.Log("发出指令 "+cmd.ToString());
            cmdQueue.Enqueue(cmd.toBytes());
        }

        public static void closeConnect()
        {

        }

        internal static void closrListener()
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
                listener.Stop();
            }

            listener = null;
            tcpClient = null;
            isConnect = false;
            pullflag = false;
            pushflag = false;
            drawpage = null;
        }
    }
}
