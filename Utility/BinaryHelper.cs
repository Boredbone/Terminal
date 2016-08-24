using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace Boredbone.Utility
{
#if !WINDOWS_APP
    /// <summary>
    /// Read/Write Binary data from/to FileStream
    /// </summary>
    public class BinaryHelper
    {


        /// <summary>
        /// Write Double value as Big-endian
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        public static void WriteDouble(FileStream stream, double value, bool isBigEndian)
        {

            var byteArray = BitConverter.GetBytes(value);

            //Big endian
            stream.Write((BitConverter.IsLittleEndian ^ isBigEndian) ? byteArray : byteArray.Reverse().ToArray(),
                0, byteArray.Length);
        }

        /// <summary>
        /// Write Int32 value as Big-endian
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        public static void WriteInt32(FileStream stream, int value, bool isBigEndian)
        {

            var byteArray = BitConverter.GetBytes(value);

            //Big endian
            stream.Write((BitConverter.IsLittleEndian ^ isBigEndian) ? byteArray : byteArray.Reverse().ToArray(),
                0, byteArray.Length);
        }

        /// <summary>
        /// Read Double value as Big-endian
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static double ReadDouble(FileStream stream, bool isBigEndian)
        {
            var length = sizeof(double);
            var buf = new byte[length];
            stream.Read(buf, 0, length);

            var value = BitConverter.ToDouble((BitConverter.IsLittleEndian ^ isBigEndian)
                ? buf : buf.Reverse().ToArray(), 0);

            return value;
        }


        /// <summary>
        /// Read Int32 value as Big-endian
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static int ReadInt32(FileStream stream, bool isBigEndian)
        {
            var length = sizeof(Int32);
            var buf = new byte[length];
            stream.Read(buf, 0, length);

            var value = BitConverter.ToInt32((BitConverter.IsLittleEndian ^ isBigEndian)
                ? buf : buf.Reverse().ToArray(), 0);

            return value;
        }

        public static int ToggleEndian(int value)
        {
            var byteArray = BitConverter.GetBytes(value);
            return BitConverter.ToInt32(byteArray.Reverse().ToArray(), 0);
        }

    }
#endif
}
