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
        public static void WriteDouble(FileStream stream,double value)
        {

            var byteArray = BitConverter.GetBytes(value);

            //Big endian
            stream.Write((BitConverter.IsLittleEndian) ? byteArray.Reverse().ToArray() : byteArray,
                0, byteArray.Length);
        }

        /// <summary>
        /// Write Int32 value as Big-endian
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        public static void WriteInt32(FileStream stream, int value)
        {

            var byteArray = BitConverter.GetBytes(value);

            //Big endian
            stream.Write((BitConverter.IsLittleEndian) ? byteArray.Reverse().ToArray() : byteArray,
                0, byteArray.Length);
        }

        /// <summary>
        /// Read Double value as Big-endian
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static double ReadDouble(FileStream stream)
        {
            var length = sizeof(double);
            var buf = new byte[length];
            stream.Read(buf, 0, length);

            var value = BitConverter.ToDouble((BitConverter.IsLittleEndian) ? buf.Reverse().ToArray() : buf, 0);

            return value;
        }


        //public static IEnumerable<double> ReadDoubleArray(FileStream stream,int count)
        //{
        //    for (int i = 0; i < count; i++)
        //    {
        //        yield return BinaryHelper.ReadDouble(stream);
        //    }
        //}


        /// <summary>
        /// Read Int32 value as Big-endian
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static int ReadInt32(FileStream stream)
        {
            var length = sizeof(Int32);
            var buf = new byte[length];
            stream.Read(buf, 0, length);

            var value = BitConverter.ToInt32((BitConverter.IsLittleEndian) ? buf.Reverse().ToArray() : buf, 0);

            return value;
        }

    }
#endif
}
