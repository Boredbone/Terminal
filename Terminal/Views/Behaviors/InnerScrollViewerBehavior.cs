using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Boredbone.Utility;
using Boredbone.Utility.Extensions;
using Boredbone.XamlTools.Extensions;
using Boredbone.XamlTools.ViewModel;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Terminal.Models;
using Terminal.Models.Macro;
using Terminal.Models.Serial;
using Terminal.Views;



namespace Terminal.Views.Behaviors
{
    class InnerScrollViewerBehavior : Behavior<FrameworkElement>
    {

        public Thickness Margin
        {
            get { return (Thickness)GetValue(MarginProperty); }
            set { SetValue(MarginProperty, value); }
        }

        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.Register(nameof(Margin), typeof(Thickness), typeof(InnerScrollViewerBehavior),
            new PropertyMetadata(new Thickness(0), new PropertyChangedCallback(OnMarginChanged)));

        private static void OnMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as InnerScrollViewerBehavior;
            var value = e.NewValue as Thickness?;

        }




        private Dictionary<string, IDisposable> Disposables { get; }
            = new Dictionary<string, IDisposable>();



        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject == null)
            {
                return;
            }

            this.AssociatedObject.LoadedAsObservable().Subscribe(_ =>
            {
                var sv = this.AssociatedObject.Descendants<ScrollViewer>().FirstOrDefault();

                if (sv != null)
                {
                    //sv.Padding = new Thickness(0, 0, 0, 64);
                    var content = sv.Content as ItemsPresenter;
                    if (content != null)
                    {
                        content.Margin = this.Margin;
                    }
                }
            })
            .AddTo(this.Disposables, "Loaded");

        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.Disposables.ForEach(y => y.Value.Dispose());
            this.Disposables.Clear();
        }

    }
}
