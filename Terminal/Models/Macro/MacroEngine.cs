﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Boredbone.Utility;
using Boredbone.Utility.Extensions;
using Boredbone.Utility.Notification;
using Reactive.Bindings.Extensions;
using Terminal.Macro.Api;
using Terminal.Models.Serial;

namespace Terminal.Models.Macro
{
    /// <summary>
    /// マクロに通信関連機能を提供
    /// </summary>
    public class MacroEngine : NotificationBase, IMacroEngine
    {
        /// <summary>
        /// タイムアウト時間，0の場合タイムアウトなし
        /// </summary>
        public int Timeout { get; set; } = 0;

        public IObservable<StatusItem> Status => this.StatusSubject.AsObservable();
        private Subject<StatusItem> StatusSubject { get; }

        private BehaviorSubject<bool> LockingSubject { get; }
        private BehaviorSubject<bool> CancelSubject { get; }

        public IObservable<bool> LogState => this.LogStateSubject.AsObservable();
        private Subject<bool> LogStateSubject { get; }


        public IObservable<string> LineReceived => this.Connection.LineReceived;

        public bool IsPausing => this.LockingSubject.Value;

        private bool isCanceled = false;

        private string nextMessage;


        private ConnectionBase Connection { get; }


        /// <summary>
        /// キャンセル要求時にOperationCanceledExceptionを発生させる
        /// </summary>
        private IObservable<WaitResultContainer> CancelObservable
            => this.CancelSubject.Where(x => x)
            .Select(x => new WaitResultContainer(new OperationCanceledException("Macro was canceled")))
            .Publish().RefCount();


        /// <summary>
        /// キャンセル要求とタイムアウト時に例外を発生させる
        /// </summary>
        private IObservable<WaitResultContainer> ExceptionObservable
            => (this.Timeout <= 0) ? this.CancelObservable
            : Observable.Timer(TimeSpan.FromMilliseconds(this.Timeout))
            .Select(x => new WaitResultContainer(new TimeoutException($"Timeout {this.Timeout} [ms]")))
            .Merge(this.CancelObservable)
            .Publish().RefCount();




        public MacroEngine(ConnectionBase connection)
        {
            this.Connection = connection;
            this.nextMessage = null;

            this.LockingSubject = new BehaviorSubject<bool>(false).AddTo(this.Disposables);
            this.StatusSubject = new Subject<StatusItem>().AddTo(this.Disposables);
            this.CancelSubject = new BehaviorSubject<bool>(false).AddTo(this.Disposables);
            this.LogStateSubject = new Subject<bool>().AddTo(this.Disposables);

        }

        /// <summary>
        /// 返信が一行来るまで待つ
        /// </summary>
        /// <returns></returns>
        public async Task<string> WaitLineAsync()
        {
            var result = await this.WaitLineAsync(1);
            return result.First();
        }

        /// <summary>
        /// 返信が指定行数来るまで待つ
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<string[]> WaitLineAsync(int count)
        {
            var list = new List<string>();
            var bs = new BehaviorSubject<bool>(false);

            var trigger = this.Connection.LineReceived
                .Skip(1)
                .Take(count)
                .Subscribe(x =>
                {
                    list.Add(x);
                    this.StatusSubject.OnNext(new StatusItem
                        ($"Wait : {count} Lines, Received : {list.Count}, {x.Replace("\n", "\\n")}"));
                },
                () =>
                {
                    bs.OnNext(true);
                });

            await this.WaitTriggerAsync(bs, trigger, new StatusItem($"Wait : {count} Lines"));

            return list.ToArray();
        }

        /// <summary>
        /// 指定文字列のいずれかが来るまで待つ
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public async Task<int> WaitAsync(params string[] keywords)
        {
            if (keywords == null
                || keywords.Length == 0
                || keywords.Any(x => (x == null || x.Length < 1)))
            {
                var r = await this.WaitAsync();
                return -1;
            }

            int result = -1;
            var bs = new BehaviorSubject<bool>(false);

            var receivedCharCountArray = new int[keywords.Length];

            var trigger = this.Connection.DataReceivedWithSendingLine
                .Select(x =>
                {
                    //受信したすべての文字に関して
                    foreach (var c in x)
                    {
                        for (int i = 0; i < keywords.Length; i++)
                        {
                            var kw = keywords[i];
                            var currentIndex = receivedCharCountArray[i];

                            var received = kw[currentIndex] == c;

                            if (!received && kw[0] == c)
                            {
                                received = true;
                                currentIndex = 0;
                            }

                            if (received)
                            {
                                currentIndex++;

                                if (currentIndex >= kw.Length)
                                {
                                    //キーワードを最後まで受信したらインデックスを返却
                                    return i;
                                }
                                receivedCharCountArray[i] = currentIndex;
                            }
                            else
                            {
                                //キーワードを受信していない
                                receivedCharCountArray[i] = 0;
                            }
                        }
                    }

                    //キーワードが含まれていない
                    return -1;
                })
                .Where(x => x >= 0)
                .Take(1)
                .Subscribe(x =>
                {
                    result = x;
                },
                () =>
                {
                    bs.OnNext(true);
                });

            await this.WaitTriggerAsync(bs, trigger, new StatusItem
                ($"Wait : {keywords.Select(x => x.Replace("\n", "\\n").Replace("\r", "\\r")).Join(",")}"));

            return result;
        }

        /// <summary>
        /// 何らかの返信が来るまで待つ
        /// </summary>
        /// <returns></returns>
        public async Task<string> WaitAsync()
        {
            string result = "";
            var bs = new BehaviorSubject<bool>(false);

            var trigger = this.Connection.DataReceived
                .Take(1)
                .Subscribe(x => result = x, () => bs.OnNext(true));

            await this.WaitTriggerAsync(bs, trigger, new StatusItem($"Wait : Any responce"));

            return result;
        }

        /// <summary>
        /// トリガーにかかるか例外が出るまで待つ
        /// </summary>
        /// <param name="bs"></param>
        /// <param name="trigger"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private async Task WaitTriggerAsync(BehaviorSubject<bool> bs, IDisposable trigger, StatusItem status)
        {

            await this.SendLineAsync(null);

            this.StatusSubject.OnNext(status);

            var waiting = await bs
                .Where(x => x)
                .Select(x => new WaitResultContainer(null))
                .Merge(this.ExceptionObservable)
                .Take(1);

            trigger.Dispose();
            bs.Dispose();

            if (!waiting.IsSucceeded)
            {
                throw waiting.Exception;
            }

            await this.WaitIfPausingAsync();
        }

        /// <summary>
        /// 指定時間待機
        /// </summary>
        /// <param name="timeMillisec"></param>
        /// <returns></returns>
        public async Task DelayAsync(int timeMillisec)
        {
            await this.SendLineAsync(null);

            this.StatusSubject.OnNext(new StatusItem($"Wait : {timeMillisec} [ms]"));
            await Task.Delay(timeMillisec);

            await this.WaitIfPausingAsync();
        }

#pragma warning disable 1998
        /// <summary>
        /// 文字列を送信
        /// </summary>
        /// <param name="text"></param>
        public async Task SendAsync(string text)
        {
            throw new NotImplementedException
                ($"Method \"{nameof(IMacroEngine.SendAsync)}\" is not implemented."
                + $" Use \"{nameof(IMacroEngine.SendLineAsync)}\"");
        }
#pragma warning restore 1998

        /// <summary>
        /// 文字列を送信
        /// </summary>
        /// <param name="text"></param>
        public async Task SendLineAsync(string text)
        {
            await this.SendLineAsync(text, false);
        }

        /// <summary>
        /// 文字列を送信
        /// </summary>
        /// <param name="text"></param>
        /// <param name="immediately"></param>
        public async Task SendLineAsync(string text, bool immediately)
        {
            if (this.nextMessage != null)
            {
                await this.SendMessageAsync(this.nextMessage);
            }

            if (immediately)
            {
                if (text != null)
                {
                    await this.SendMessageAsync(text);
                }
                this.nextMessage = null;
            }
            else
            {
                this.nextMessage = text;
            }
        }

        /// <summary>
        /// 文字列をConnectionに渡す
        /// </summary>
        /// <param name="text"></param>
        private async Task SendMessageAsync(string text)
        {
            await this.WaitIfPausingAsync();

            this.StatusSubject.OnNext(new StatusItem("Send : " + text));
            this.Connection.WriteLine(text);

            await this.WaitIfPausingAsync();

        }

        /// <summary>
        /// 受信履歴
        /// </summary>
        /// <param name="back">さかのぼる行数</param>
        /// <returns>指定された行の受信文字列</returns>
        public string History(int back) => this.Connection.History(back);

        /// <summary>
        /// 文字列を画面に表示
        /// </summary>
        /// <param name="text"></param>
        public void Display(string text)
        {
            this.StatusSubject.OnNext(new StatusItem(text) { Type = StatusType.Message });
        }

        /// <summary>
        /// マクロ開始
        /// </summary>
        /// <param name="name"></param>
        public void Start(string name)
        {
            this.StatusSubject.OnNext(new StatusItem($"Macro {name} loading"));
        }

        /// <summary>
        /// マクロ終了
        /// </summary>
        /// <param name="name"></param>
        public void End(string name)
        {
            this.StatusSubject.OnNext(new StatusItem($"Macro {name} end"));
        }

        /// <summary>
        /// 一時停止
        /// </summary>
        public void Pause()
        {
            this.LockingSubject.OnNext(true);
        }

        /// <summary>
        /// 再開
        /// </summary>
        public void Resume()
        {
            this.LockingSubject.OnNext(false);
        }

        /// <summary>
        /// 一時停止中の場合は待機
        /// </summary>
        /// <returns></returns>
        private async Task WaitIfPausingAsync()
        {
            var waiting = await this.LockingSubject
                .Where(x => !x)
                .Select(x => new WaitResultContainer(null))
                .Merge(this.CancelObservable)
                .Take(1);

            if (!waiting.IsSucceeded)
            {
                throw waiting.Exception;
            }
        }

        /// <summary>
        /// 実行を停止
        /// </summary>
        public void Cancel()
        {
            if (!this.isCanceled)
            {
                Task.Run(() =>
                {
                    this.isCanceled = true;
                    this.CancelSubject.OnNext(true);
                });
            }
        }

        /// <summary>
        /// キャンセル状態の解除
        /// </summary>
        public void ClearCancellation()
        {
            this.CancelSubject.OnNext(false);
            this.LockingSubject.OnNext(false);
        }

        /// <summary>
        /// ログの追尾をセット
        /// </summary>
        /// <param name="state"></param>
        public void SetLogState(bool state)
        {
            this.LogStateSubject.OnNext(state);
        }


        /// <summary>
        /// 接続設定を取得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetSetting(string key)
            => this.Connection.GetSetting(key);

        /// <summary>
        /// 接続設定を書き換え
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetSetting(string key, string value)
            => this.Connection.SetSetting(key, value);


        /// <summary>
        /// 待機結果のコンテナ
        /// </summary>
        private class WaitResultContainer
        {
            public bool IsSucceeded => this.Exception == null;
            public Exception Exception { get; }

            public WaitResultContainer(Exception exception)
            {
                this.Exception = exception;
            }
        }
    }
}
