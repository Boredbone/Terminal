using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Terminal.Views.Behaviors
{
    public class ImageGrayoutBehavior : Behavior<Image>
    {

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.IsEnabledChanged += OnIsEnabledChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.IsEnabledChanged -= OnIsEnabledChanged;
        }

        /// <summary>
        /// Called when [auto grey scale image is enabled property changed].
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="args">The instance containing the event data.</param>
        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var isEnable = Convert.ToBoolean(args.NewValue);

            if (this.AssociatedObject != null)
            {

                if (!isEnable && this.AssociatedObject.Source != null)
                {
                    // Get the source bitmap
                    var bitmapImage = new BitmapImage(new Uri(this.AssociatedObject.Source.ToString()));

                    // Convert it to Gray
                    this.AssociatedObject.Source = new FormatConvertedBitmap
                        (bitmapImage, PixelFormats.Gray32Float, null, 0);

                    // Create Opacity Mask for greyscale image
                    // as FormatConvertedBitmap does not keep transparency info
                    this.AssociatedObject.OpacityMask = new ImageBrush(bitmapImage);

                    this.AssociatedObject.Opacity = 0.25;
                }
                else
                {
                    if (this.AssociatedObject.Source != null)
                    {
                        // Set the Source property to the original value.
                        this.AssociatedObject.Source = ((FormatConvertedBitmap)this.AssociatedObject.Source).Source;
                    }

                    // Reset the Opcity Mask
                    this.AssociatedObject.OpacityMask = null;
                    this.AssociatedObject.Opacity = 1.0;
                }
            }
        }
    }
}
