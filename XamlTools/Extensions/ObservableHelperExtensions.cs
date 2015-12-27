using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using System.Reactive.Subjects;
#if WINDOWS_APP || WINDOWS_UWP
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Boredbone.XamlTools.Extensions
{
    public static class ObservableHelperExtensions
    {


        /// <summary>
        /// 辞書にIDisposableを追加し、同じキーが存在している場合は元のアイテムをDispose
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="disposable"></param>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T AddTo<T, TKey>
            (this T disposable, IDictionary<TKey, IDisposable> dictionary, TKey key) where T : IDisposable
        {
            IDisposable result;
            if (dictionary.TryGetValue(key, out result))
            {
                result?.Dispose();
                dictionary.Remove(key);
            }

            dictionary.Add(key, disposable);

            return disposable;
        }

        /// <summary>
        /// 一定時間ごとに一つだけ通過
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IObservable<T> DownSample<T>(this IObservable<T> source, TimeSpan interval)
        {
            return Observable.Create<T>(o =>
            {
                var subscriptions = new CompositeDisposable();
                var acceepted = true;
                
                var pub = source.Where(x => acceepted);

                pub.Subscribe(o).AddTo(subscriptions);

                pub.Do(x => acceepted = false)
                .Delay(interval).Subscribe(x => acceepted = true)
                .AddTo(subscriptions);

                return subscriptions;
            });
        }

        public static ReactiveCommand<Tvalue> WithSubscribe<Tvalue>
            (this ReactiveCommand<Tvalue> observable, Action<Tvalue> action, ICollection<IDisposable> container)
        {
            observable.Subscribe(action).AddTo(container);
            observable.AddTo(container);
            return observable;
        }
        public static ReactiveCommand WithSubscribe
            (this ReactiveCommand observable, Action<object> action, ICollection<IDisposable> container)
        {
            observable.Subscribe(action).AddTo(container);
            observable.AddTo(container);
            return observable;
        }
        public static Subject<Tvalue> WithSubscribe<Tvalue>
            (this Subject<Tvalue> observable, Action<Tvalue> action, ICollection<IDisposable> container)
        {
            observable.Subscribe(action).AddTo(container);
            observable.AddTo(container);
            return observable;
        }

        public static bool Toggle(this ReactiveProperty<bool> target)
        {
            var newValue = !target.Value;
            target.Value = newValue;
            return newValue;
        }


        public static IObservable<SizeChangedEventArgs> SizeChangedAsObservable(this FrameworkElement target)
            => Observable.FromEvent<SizeChangedEventHandler, SizeChangedEventArgs>
                (h => (sender, e) => h(e), h => target.SizeChanged += h, h => target.SizeChanged -= h);



        public static IObservable<RoutedEventArgs> LoadedAsObservable(this FrameworkElement target)
            => Observable.FromEvent<RoutedEventHandler, RoutedEventArgs>
                (h => (sender, e) => h(e), h => target.Loaded += h, h => target.Loaded -= h);
    }
}
