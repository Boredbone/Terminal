using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Terminal.ViewModels
{
    public enum LogTypes
    {
        Normal,
        Error,
        Echo,
        Notice,
        DisabledMessage,
        MacroMessage,
    }

    public static class LogTypeExtensions
    {

        public static Brush GetColor(this LogTypes type)
        {
            switch (type)
            {
                case LogTypes.Normal:
                    return Brushes.Black;
                case LogTypes.Error:
                    return Brushes.Red;
                case LogTypes.Echo:
                    return Brushes.Blue;
                case LogTypes.Notice:
                    return Brushes.Green;
                case LogTypes.DisabledMessage:
                    return Brushes.Gray;
                case LogTypes.MacroMessage:
                    return Brushes.Blue;
                default:
                    return Brushes.Black;
            }
        }
    }
}
