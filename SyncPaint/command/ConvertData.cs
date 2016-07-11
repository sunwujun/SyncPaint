using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncPaint.command
{
    class ConvertData
    {


        public static int bytesToInt(byte[] data, int start)
        {
            return BitConverter.ToInt32(data, start);
     //       return (data[start + 3] & 0xFF) << 24 | (data[start + 2] & 0xFF) << 16 | (data[start + 1] & 0xFF) << 8 | (data[start] & 0xFF);
        }

        public static void FloatTobytes(byte[] data, int start, float source)
        {
            byte[] temp = BitConverter.GetBytes(source);
            for (int i = 0; i < temp.Length; i++)
            {
                data[start + i] = temp[i];
            }
       
        }

        public static float bytesToFloat(byte[] data, int start)
        {
            float f= BitConverter.ToSingle(data, start);
            return f;
        }

        public static void IntTobytes(byte[] data, int start, int source)
        {
           byte[]temp=BitConverter.GetBytes(source);
            for(int i = 0; i < temp.Length; i++)
            {
                data[start + i] = temp[i];
            }
           
        }

    }
}
