﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Boredbone.Utility.Extensions;

namespace Terminal.Models.Serial
{
    public abstract class ConnectionBase : IDisposable
    {

        private LineCodes _fieldLineCode;
        public LineCodes LineCode
        {
            get { return _fieldLineCode; }
            set
            {
                _fieldLineCode = value;
                if (value == LineCodes.Cr)
                {
                    this.Splitter = new[] { "\r" };
                    this.ReceivingNewLine = "\r";
                    this.IgnoredNewLine = "\n";
                }
                else
                {
                    this.Splitter = new[] { "\n" };
                    this.ReceivingNewLine = "\n";
                    this.IgnoredNewLine = "\r";
                }
            }
        }


        public string[] Splitter { get; private set; }
        public string ReceivingNewLine { get; private set; }
        public string IgnoredNewLine { get; private set; }


        public string SendingNewLine { get; set; } = "\r\n";
        public Encoding Encoding { get; set; } = Encoding.GetEncoding("Shift_JIS");

        public string PortName { get; private set; }


        private CircularBuffer<string> HistoryList { get; }
        private string LineBuffer { get; set; }
        
        protected Dictionary<string,SettingAccessor> Settings { get; }

        /// <summary>
        /// ポート変更通知
        /// </summary>
        public IObservable<bool> Portchanged => this.PortChangedSubject.AsObservable();
        protected Subject<bool> PortChangedSubject { get; }

        /// <summary>
        /// ポートOpen/Close通知
        /// </summary>
        public IObservable<bool> IsOpenChanged => this.IsOpenProperty.AsObservable();
        public bool IsOpen => this.IsOpenProperty.Value;
        private ReactiveProperty<bool> IsOpenProperty { get; }

        /// <summary>
        /// データ受信通知
        /// </summary>
        public IObservable<string> DataReceived => this.DataReceivedSubject.AsObservable();
        protected Subject<string> DataReceivedSubject { get; }

        /// <summary>
        /// データ送信による改行を含むデータ受信通知
        /// </summary>
        public IObservable<string> DataReceivedWithSendingLine { get; }


        /// <summary>
        /// 一行受信
        /// </summary>
        public IObservable<string> LineReceived
            => this.LineReceivedSubject
            .Buffer(this.DataReceivedWithSendingLine)
            .SelectMany(y => y)
            .Publish()
            .RefCount();
        private Subject<string> LineReceivedSubject { get; }

        /// <summary>
        /// ローカルエコー
        /// </summary>
        public IObservable<string> DataSent => this.DataSentSubject.AsObservable();
        private Subject<string> DataSentSubject { get; }

        /// <summary>
        /// データ送信失敗
        /// </summary>
        public IObservable<string> DataIgnored => this.DataIgnoredSubject.AsObservable();
        private Subject<string> DataIgnoredSubject { get; }

        protected ObservableCollection<string> portNames;
        public ReadOnlyReactiveCollection<string> PortNames
            => this.portNames.ToReadOnlyReactiveCollection();
        
        private CompositeDisposable Disposables { get; }

        protected abstract bool IsPortEnabled { get; }

        public ConnectionBase()
        {
            this.Disposables = new CompositeDisposable();

            this.PortName = "";

            this.PortChangedSubject = new Subject<bool>().AddTo(this.Disposables);
            this.DataReceivedSubject = new Subject<string>().AddTo(this.Disposables);
            this.DataIgnoredSubject = new Subject<string>().AddTo(this.Disposables);
            this.LineReceivedSubject = new Subject<string>().AddTo(this.Disposables);
            this.DataSentSubject = new Subject<string>().AddTo(this.Disposables);

            this.IsOpenProperty = new ReactiveProperty<bool>(false).AddTo(this.Disposables);

            this.LineCode = LineCodes.Lf;

            this.HistoryList = new CircularBuffer<string>(1024);
            this.LineBuffer = "";

            this.portNames = new ObservableCollection<string>();

            this.Settings = new Dictionary<string, SettingAccessor>();

            this.Settings.Add(nameof(this.Encoding), new SettingAccessor(
                x =>
                {
                    try
                    {
                        var encoding = Encoding.GetEncoding(x);
                        this.Encoding = encoding;
                    }
                    catch { }
                },
                () => this.Encoding.WebName));


            this.DataReceivedWithSendingLine = this.DataReceivedSubject
                .Merge(this.DataSentSubject.Select(x => this.ReceivingNewLine));
            

            //受信データを一行ごとに整形
            this.DataReceivedWithSendingLine
                .Buffer(this.DataReceivedWithSendingLine.Where(x => x.Contains(this.ReceivingNewLine)))
                .Subscribe(list =>
                {
                    var fixedText = this.LineBuffer + list.Join();
                    var texts = fixedText.Split(this.Splitter, StringSplitOptions.None);

                    if (texts.Length > 0)
                    {
                        for (int i = 0; i < texts.Length - 1; i++)
                        {
                            this.HistoryList.Add(texts[i]);
                            this.LineReceivedSubject.OnNext(texts[i]);
                        }
                        this.LineBuffer = texts[texts.Length - 1];
                    }

                })
                .AddTo(this.Disposables);
            
            
        }
        
        /// <summary>
        /// 受信データ履歴
        /// </summary>
        /// <param name="back"></param>
        /// <returns></returns>
        public string History(int back)
        {

            if (back <= 0)
            {
                return this.LineBuffer;
            }
            if (back >= this.HistoryList.Length)
            {
                return "";
            }
            return this.HistoryList.GetPrevious(back - 1) ?? "";

            //var index = this.HistoryList.Count - back;
            //return (index >= 0) ? this.HistoryList[index] : "";

        }

        /// <summary>
        /// シリアルポートを開く
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public void Open(string name)
        {
            //すでにポートを開いていたら例外
            if (this.IsPortEnabled)
            {
                throw new InvalidOperationException("Already connected");
            }

            this.OnOpening(name);

            this.PortName = name;
            this.IsOpenProperty.Value = true;
        }

        /// <summary>
        /// 送信
        /// </summary>
        /// <param name="text"></param>
        public void WriteLine(string text)
        {
            if (this.IsPortEnabled)
            {
                this.OnSending(text);
                this.DataSentSubject.OnNext(text + this.ReceivingNewLine);
            }
            else
            {
                this.DataIgnoredSubject.OnNext(text);
            }
        }


        /// <summary>
        /// ポートを破棄
        /// </summary>
        public void Dispose()
        {
            this.Close();
            this.Disposables.Dispose();
        }

        /// <summary>
        /// ポートを閉じる
        /// </summary>
        public void Close()
        {
            this.OnClosing();
            this.IsOpenProperty.Value = false;
        }

        public IEnumerable<KeyValuePair<string, string>> GetSettings()
            => this.Settings.ToDictionary(x => x.Key, x => x.Value.Value);

        public string GetSetting(string key)
        {
            if (key == null)
            {
                return null;
            }

            SettingAccessor item = null;
            if (this.Settings.TryGetValue(key, out item))
            {
                if (item != null)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public void SetSetting(string key, string value)
        {
            if (value == null)
            {
                return;
            }
            SettingAccessor item = null;
            if (this.Settings.TryGetValue(key, out item))
            {
                if (item != null)
                {
                    item.Value = value;
                }
            }
        }
            
        

        /// <summary>
        /// ポート名一覧取得
        /// </summary>
        /// <returns></returns>
        //public abstract string[] GetPortNames();

        public abstract void RefreshPortNames();


        protected abstract void OnOpening(string name);

        protected abstract void OnSending(string text);

        protected abstract void OnClosing();

        protected void AddToDisposables(IDisposable disposable)
        {
            disposable.AddTo(this.Disposables);
        }
        
        protected class SettingAccessor
        {
            private Action<string> setter;
            private Func<string> getter;

            public string Value
            {
                get { return this.getter(); }
                set { this.setter(value); }
            }

            public SettingAccessor(Action<string> setter, Func<string> getter)
            {
                this.setter = setter;
                this.getter = getter;
            }
        }
    }
}
