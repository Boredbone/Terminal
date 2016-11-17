using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Boredbone.Utility.Extensions;
using Reactive.Bindings.Extensions;

namespace Terminal.ViewModels
{

    internal static class ObservableExtensions
    {

        private static void Run<T>(List<T> buffer, IObserver<IList<T>> observer, object gate,
            TimeSpan prevInterval, TimeSpan minInterval, TimeSpan maxInterval,
            double coefficient,
            SerialDisposable subscription, IScheduler scheduler)
        {

            double time;
            int count;

            lock (gate)
            {
                var list = new List<T>(buffer);
                buffer.Clear();
                count = list.Count;
                time = count / prevInterval.TotalMilliseconds * coefficient;
                if (count > 0)
                {
                    observer.OnNext(list);
                }
            }

            var interval = TimeSpan.FromMilliseconds(time);

            if (interval < minInterval)
            {
                interval = minInterval;
            }
            else if (interval > maxInterval)
            {
                interval = maxInterval;
            }

            var d = new SingleAssignmentDisposable();
            subscription.Disposable = d;

            d.Disposable = scheduler.Schedule(0, interval, (self, _) =>
            {
                //Debug.WriteLine($"{DateTime.Now},{interval.TotalMilliseconds},{count}");
                Run(buffer, observer, gate,
                    interval, minInterval, maxInterval,
                    coefficient, subscription, scheduler);
                return Disposable.Empty;
            });

        }

        public static IObservable<IList<T>> BufferVariable2<T>
            (this IObservable<T> source, TimeSpan minInterval, TimeSpan maxInterval,
            double coefficient, IScheduler scheduler)
        {
            return Observable.Create<IList<T>>(observer =>
            {
                object gate = new object();



                var ds = new CompositeDisposable();

                var subscription = new SerialDisposable().AddTo(ds);

                var subject = new Subject<IList<T>>().AddTo(ds);

                var buffer = new List<T>();

                Run(buffer, subject, gate,
                    minInterval, minInterval, maxInterval,
                    coefficient, subscription, scheduler);

                source.Subscribe(value =>
                {
                    lock (gate)
                    {
                        buffer.Add(value);
                    }
                }).AddTo(ds);


                subject.Subscribe(observer).AddTo(ds);
                return ds;
            });

        }

        public static IObservable<IList<T>> BufferVariable<T>
            (this IObservable<T> source, TimeSpan minInterval, TimeSpan maxInterval,
            double coefficient, IScheduler scheduler)
        {
            var gate = new object();

            var subscription = new CompositeDisposable();

            var cancelable = new CompositeDisposable().AddTo(subscription);

            var prevTime = default(DateTimeOffset);

            var buffer = new List<T>();


            var timeBase = 0.0;


            return Observable.Create<IList<T>>(observer =>
            {
                source.Subscribe(x =>
                {
                    TimeSpan interval;
                    var disposable = new SingleAssignmentDisposable();

                    lock (gate)
                    {
                        buffer.Add(x);
                        interval = TimeSpan.FromMilliseconds(timeBase);
                        cancelable.Add(disposable);
                    }

                    //var interval = TimeSpan.FromMilliseconds(time);

                    if (interval < minInterval)
                    {
                        interval = minInterval;
                    }
                    else if (interval > maxInterval)
                    {
                        interval = maxInterval;
                    }

                    /*
                    if (elapsed > 0)
                    {
                        interval = TimeSpan.FromMilliseconds(coefficient / elapsed);

                        if (interval < minInterval)
                        {
                            interval = minInterval;
                        }
                        else if (interval > maxInterval)
                        {
                            interval = maxInterval;
                        }
                    }
                    else
                    {
                        interval = maxInterval;
                    }*/

                    //Debug.WriteLine($"{DateTime.Now},{interval.TotalMilliseconds},{elapsed}");

                    disposable.Disposable = scheduler.Schedule(0, interval, (self, _) =>
                    {
                        int count;
                        List<T> list;

                        var currentTime = scheduler.Now;
                        //Debug.WriteLine(cancelable.Count);
                        //double t = 0.0;

                        lock (gate)
                        {
                            list = new List<T>(buffer);
                            buffer.Clear();
                            var prevInterval = currentTime - prevTime;
                            prevTime = currentTime;

                            count = list.Count;
                            cancelable.Clear();

                            if (count > 0)
                            {
                                var time = prevInterval.TotalMilliseconds;
                                if (time <= 0)
                                {
                                    time = 1;
                                }
                                timeBase = count / time * coefficient;
                                //t = time;
                                observer.OnNext(list);
                            }
                        }
                        //if (count > 0)
                        //{
                        //
                        //    Debug.WriteLine($"{timeBase:f1} p {count} / {t:f1}");
                        //}
                        //Debug.WriteLine($"{DateTime.Now},{interval.TotalMilliseconds},{elapsed}");


                        return Disposable.Empty;
                    });

                })
                .AddTo(subscription);

                return subscription;

            });

        }
    }
}
