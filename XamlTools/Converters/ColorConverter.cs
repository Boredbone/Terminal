#if !WINDOWS_APP && !WINDOWS_UWP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Boredbone.XamlTools.Converters
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {

            var code = value as uint?;

            if (code != null)
            {
                var a = (byte)((code.Value & 0xff000000) >> 24);
                var r = (byte)((code.Value & 0x00ff0000) >> 16);
                var g = (byte)((code.Value & 0x0000ff00) >> 8);
                var b = (byte)((code.Value & 0x000000ff) >> 0);

                return new SolidColorBrush(Color.FromArgb(a, r, g, b));
            }


            var array = value as byte[];

            if (array == null || array.Length < 4)
            {
                return new SolidColorBrush(Colors.Transparent);
            }
            return new SolidColorBrush(Color.FromArgb(array[0], array[1], array[2], array[3]));
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

#endif
