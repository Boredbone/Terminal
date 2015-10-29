﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Boredbone.Utility.Extensions;
using Boredbone.XamlTools.ViewModel;
using Reactive.Bindings.Extensions;
using Terminal.Models.Serial;

namespace Terminal.Models.Macro
{
    /// <summary>
    /// マクロの実行を管理
    /// </summary>
    public class MacroPlayer : ViewModelBase
    {
        private MacroEngine Engine { get; set; }
        private IConnection Connaction { get; }

        private Subject<string> ErrorSubject { get; }
        public IObservable<string> Error => this.ErrorSubject.AsObservable();

        //private Subject<string> NoticeSubject { get; }
        //public IObservable<string> Notice => this.NoticeSubject.AsObservable();

        private Subject<StatusItem> MessageSubject { get; }
        public IObservable<StatusItem> Message => this.MessageSubject.AsObservable();


        private BehaviorSubject<bool> IsExecutingSubject { get; }
        public IObservable<bool> IsExecuting => this.IsExecutingSubject.AsObservable();


        private BehaviorSubject<bool> IsPausingSubject { get; }
        public IObservable<bool> IsPausing => this.IsPausingSubject.AsObservable();


        public MacroPlayer(IConnection connection)
        {
            this.Connaction = connection;
            this.ErrorSubject = new Subject<string>().AddTo(this.Disposables);
            //this.NoticeSubject = new Subject<string>().AddTo(this.Disposables);
            this.MessageSubject = new Subject<StatusItem>().AddTo(this.Disposables);
            this.IsExecutingSubject = new BehaviorSubject<bool>(false).AddTo(this.Disposables);
            this.IsPausingSubject = new BehaviorSubject<bool>(false).AddTo(this.Disposables);
        }

        /// <summary>
        /// マクロを開始
        /// </summary>
        /// <param name="code"></param>
        public void Start(IMacroCode code)
        {

            if (this.Engine != null)
            {
                throw new InvalidOperationException("Already running");
            }

            this.IsExecutingSubject.OnNext(true);
            this.IsPausingSubject.OnNext(false);

            var macroDisposables = new CompositeDisposable();
            var macro = new MacroEngine(this.Connaction).AddTo(macroDisposables);
            this.Engine = macro;

            macro.Status
            //.ObserveOnUIDispatcher()
            .Subscribe(y =>
            {
                this.MessageSubject.OnNext(y);
            })
            .AddTo(macroDisposables);


            //var mc = new MacroCode();

            //別スレッドで開始
            Task.Run(async () =>
            {
                try
                {
                    await code.RunAsync(macro);
                }
                catch (Exception e) when (e is TimeoutException || e is OperationCanceledException)
                {
                    this.ErrorSubject.OnNext(e.GetType().FullName + ": " + e.Message);
                }
                catch (Exception e)
                {
                    this.ErrorSubject.OnNext(e.ToString());
                }
                finally
                {
                    macroDisposables.Dispose();
                    this.Engine = null;
                    this.IsExecutingSubject.OnNext(false);
                    this.IsPausingSubject.OnNext(false);
                }
            })
            .FireAndForget(e =>
            {
                this.ErrorSubject.OnNext("Error : " + e.GetType().FullName + ": " + e.Message);
            });
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Cancel()
        {
            this.Engine?.Cancel();
        }

        /// <summary>
        /// 一時停止または再開
        /// </summary>
        public void PauseOrResume()
        {
            if (this.Engine == null)
            {
                return;
            }

            if (this.Engine.IsPausing)
            {
                this.IsPausingSubject.OnNext(false);
                this.Engine.Resume();
            }
            else
            {
                this.IsPausingSubject.OnNext(true);
                this.Engine.Pause();
            }

        }
    }
}
