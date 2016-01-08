using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Terminal.Views.Behaviors
{
    public class ColorChangeBehavior : Behavior<Panel>
    {

        public bool Flag
        {
            get { return (bool)GetValue(FlagProperty); }
            set { SetValue(FlagProperty, value); }
        }

        public static readonly DependencyProperty FlagProperty =
            DependencyProperty.Register(nameof(Flag), typeof(bool), typeof(ColorChangeBehavior),
            new PropertyMetadata(false, new PropertyChangedCallback(OnFlagChanged)));

        private static void OnFlagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as ColorChangeBehavior;
            var value = e.NewValue as bool?;

            if (value == null)
            {
                return;
            }

            thisInstance.ChangeColor();
        }


        public Brush TrueColor
        {
            get { return (Brush)GetValue(TrueColorProperty); }
            set { SetValue(TrueColorProperty, value); }
        }

        public static readonly DependencyProperty TrueColorProperty =
            DependencyProperty.Register(nameof(TrueColor), typeof(Brush),
                typeof(ColorChangeBehavior), new PropertyMetadata(Brushes.Transparent));

        public Brush FalseColor
        {
            get { return (Brush)GetValue(FalseColorProperty); }
            set { SetValue(FalseColorProperty, value); }
        }

        public static readonly DependencyProperty FalseColorProperty =
            DependencyProperty.Register(nameof(FalseColor), typeof(Brush),
                typeof(ColorChangeBehavior), new PropertyMetadata(Brushes.Transparent));
        





        private void ChangeColor()
        {
            if (this.AssociatedObject == null)
            {
                return;
            }
            this.AssociatedObject.Background = this.Flag ? this.TrueColor : this.FalseColor;
        }




        protected override void OnAttached()
        {
            base.OnAttached();
            this.ChangeColor();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
        
    }
}
