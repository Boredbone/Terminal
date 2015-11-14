using System;
using System.Collections.Generic;
using System.Text;
#if WINDOWS_APP || WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
#endif

namespace Boredbone.XamlTools.Converters
{
    /// <summary>
    /// Converts a Boolean into a Visibility.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// If set to True, conversion is reversed: True will become Collapsed.
        /// </summary>
        public bool IsReversed { get; set; }

        public object Convert(object value, Type targetType, object parameter,
#if WINDOWS_APP || WINDOWS_UWP
            string language
#else
            System.Globalization.CultureInfo culture
#endif
            )
        {
            var val = System.Convert.ToBoolean(value);
            if (this.IsReversed)
            {
                val = !val;
            }

            if (val)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
#if WINDOWS_APP || WINDOWS_UWP
            string language
#else
            System.Globalization.CultureInfo culture
#endif
            )
        {
            throw new NotImplementedException();
        }

    }

    public class BooleanToOpacityConverter : IValueConverter
    {
        /// <summary>
        /// If set to True, conversion is reversed: True will become Collapsed.
        /// </summary>
        public bool IsReversed { get; set; }

        public object Convert(object value, Type targetType, object parameter,
#if WINDOWS_APP || WINDOWS_UWP
            string language
#else
            System.Globalization.CultureInfo culture
#endif
            )
        {
            var val = System.Convert.ToBoolean(value);
            if (this.IsReversed)
            {
                val = !val;
            }

            if (val)
            {
                return 1.0;
            }

            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
#if WINDOWS_APP || WINDOWS_UWP
            string language
#else
            System.Globalization.CultureInfo culture
#endif
            )
        {
            throw new NotImplementedException();
        }

    }
}
