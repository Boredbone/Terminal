using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Terminal.Views
{
    /// <summary>
    /// VerticalImageIcon.xaml の相互作用ロジック
    /// </summary>
    public partial class ImageButton : UserControl
    {

        public Button Button => this.button;

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(ImageButton),
            new PropertyMetadata(null, new PropertyChangedCallback(OnTextChanged)));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as ImageButton;
            var value = e.NewValue as string;

        }

        public object ButtonContent
        {
            get { return (object)GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }

        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register(nameof(ButtonContent), typeof(object), typeof(ImageButton),
            new PropertyMetadata(null, new PropertyChangedCallback(OnButtonContentChanged)));

        private static void OnButtonContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as ImageButton;
            //var value = e.NewValue as object;
            thisInstance.button.Content = e.NewValue;
        }

        public ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }

        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register(nameof(ButtonCommand), typeof(ICommand), typeof(ImageButton),
            new PropertyMetadata(null, new PropertyChangedCallback(OnButtonCommandChanged)));

        private static void OnButtonCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as ImageButton;
            var value = e.NewValue as ICommand;
            if (value != null)
            {
                thisInstance.button.Command = value;
            }
        }


        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation),
                typeof(ImageButton), new PropertyMetadata(Orientation.Vertical));

        //private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var thisInstance = d as VerticalImageIcon;
        //    var value = e.NewValue as Orientation;
        //
        //}





        public ImageButton()
        {
            InitializeComponent();
        }
    }
}
