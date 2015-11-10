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
using Terminal.Macro.Api;
using Terminal.Models.Serial;

namespace Terminal.Models.Macro
{
    /// <summary>
    /// マクロの実行を管理
    /// </summary>
    public class MacroPlayer : ViewModelBase, IMacroPlayer
    {
        IMacroEngine IMacroPlayer.Engine => this.Engine;
        IModuleManager IMacroPlayer.Modules => this.Modules;

        public MacroEngine Engine { get; private set; }
        private ConnectionBase Connaction { get; }

        public ModuleManager Modules { get; }

        private Subject<string> ErrorSubject { get; }
        public IObservable<string> Error => this.ErrorSubject.AsObservable();
        

        private Subject<StatusItem> MessageSubject { get; }
        public IObservable<StatusItem> Message => this.MessageSubject.AsObservable();


        private BehaviorSubject<bool> IsExecutingSubject { get; }
        public IObservable<bool> IsExecutingChanged => this.IsExecutingSubject.AsObservable();
        public bool IsExecuting => this.IsExecutingSubject.Value;


        private BehaviorSubject<bool> IsPausingSubject { get; }
        public IObservable<bool> IsPausing => this.IsPausingSubject.AsObservable();


        public MacroPlayer(ConnectionBase connection)
        {
            this.Connaction = connection;
            this.ErrorSubject = new Subject<string>().AddTo(this.Disposables);
            this.MessageSubject = new Subject<StatusItem>().AddTo(this.Disposables);
            this.IsExecutingSubject = new BehaviorSubject<bool>(false).AddTo(this.Disposables);
            this.IsPausingSubject = new BehaviorSubject<bool>(false).AddTo(this.Disposables);
            
            this.Modules = new ModuleManager();
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
            .Subscribe(y =>
            {
                this.MessageSubject.OnNext(y);
            })
            .AddTo(macroDisposables);

            this.Modules.Engine = macro;
            

            //別スレッドで開始
            Task.Run(async () =>
            {
                macro.Start(code.Name);

                try
                {
                    await code.RunAsync(macro, this.Modules);
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
                    macro.End(code.Name);
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
        

        void IMacroPlayer.Start(string name, Func<IMacroEngine, IModuleManager, Task> asyncFunc)
        {
            this.Start(new DelegateMacro(name, asyncFunc));
        }
        
    }
}
