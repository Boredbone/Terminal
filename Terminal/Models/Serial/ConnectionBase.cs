using System;
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

namespace Terminal.Models.Serial
{
    public abstract class ConnectionBase
    {
        private string ValidPortName = "Valid";
        private string InvalidPortName = "Invalid";

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
        //public int BaudRate { get; set; } = 9600;
        //public int DataBits { get; set; } = 8;

        public string PortName { get; private set; }

        private ConnectionHistory ConnectionHistory { get; }


        /// <summary>
        /// ポート変更通知
        /// </summary>
        private Subject<bool> PortChangedSubject { get; }
        public IObservable<bool> Portchanged => this.PortChangedSubject.AsObservable();

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
        private Subject<string> DataReceivedSubject { get; }

        /// <summary>
        /// データ送信による改行を含むデータ受信通知
        /// </summary>
        public IObservable<string> DataReceivedWithSendingLine => this.ConnectionHistory.DataReceivedWithSendingLine;

        /// <summary>
        /// 一行受信
        /// </summary>
        public IObservable<string> LineReceived => this.ConnectionHistory.LineReceived;//this.LineReceivedSubject.AsObservable();
        //private Subject<string> LineReceivedSubject { get; }

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


        private CompositeDisposable ConnectionDisposables { get; }
        private CompositeDisposable Disposables { get; }



        public ConnectionBase()
        {
            this.Disposables = new CompositeDisposable();
            this.ConnectionDisposables = new CompositeDisposable().AddTo(this.Disposables);

            this.PortName = "";

            this.PortChangedSubject = new Subject<bool>().AddTo(this.Disposables);
            this.DataReceivedSubject = new Subject<string>().AddTo(this.Disposables);
            this.DataIgnoredSubject = new Subject<string>().AddTo(this.Disposables);
            //this.LineReceivedSubject = new Subject<string>().AddTo(this.Disposables);
            this.DataSentSubject = new Subject<string>().AddTo(this.Disposables);

            this.IsOpenProperty = new ReactiveProperty<bool>(false).AddTo(this.Disposables);

            this.LineCode = LineCodes.Lf;

            this.ConnectionHistory = new ConnectionHistory(this).AddTo(this.Disposables);

            //this.DataReceivedWithSendingLine = this.DataReceivedSubject
            //    .Merge(this.DataSentSubject.Select(x => this.ReceivingNewLine));
            //
            //var buffer = "";
            //
            ////受信データを一行ごとに整形
            //this.DataReceivedWithSendingLine
            //    .Buffer(this.DataReceivedWithSendingLine.Where(x => x.Contains(this.ReceivingNewLine)))
            //    .Subscribe(list =>
            //    {
            //        var fixedText = buffer + list.Join();
            //        var texts = fixedText.Split(this.Splitter, StringSplitOptions.None);
            //
            //        if (texts.Length > 0)
            //        {
            //            for (int i = 0; i < texts.Length - 1; i++)
            //            {
            //                this.LineReceivedSubject.OnNext(texts[i]);
            //            }
            //            buffer = texts[texts.Length - 1];
            //        }
            //
            //    })
            //    .AddTo(this.Disposables);

            this.DataSentSubject
                .Delay(TimeSpan.FromMilliseconds(2000))
                .Select(x => "echo:" + x + (x.Length > 0 ? "\n>" : ">"))
                //.Select(x => ">")//"echo:" + x + (x.Length > 0 ? "\n>" : ">"))
                .Subscribe(this.DataReceivedSubject)
                .AddTo(this.Disposables);

        }

        public string History(int back) => this.ConnectionHistory.History(back);

        /// <summary>
        /// シリアルポートを開く
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public void Open(string name)
        {
            //すでにポートを開いていたら例外
            if (this.IsOpen)
            {
                throw new InvalidOperationException("Already connected");
            }


            this.ConnectionDisposables.Clear();

            if (name.Equals(this.ValidPortName))
            {
                this.PortName = name;
                this.IsOpenProperty.Value = true;
                this.DataReceivedSubject.OnNext(">");
            }
            else
            {
                throw new ArgumentException("Invalid name");
            }
        }

        /// <summary>
        /// 送信
        /// </summary>
        /// <param name="text"></param>
        public void WriteLine(string text)
        {
            if (this.IsOpen)
            {
                this.DataSentSubject.OnNext(text);
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
            this.IsOpenProperty.Value = false;
            this.ConnectionDisposables.Clear();
        }

        /// <summary>
        /// ポート名一覧取得
        /// </summary>
        /// <returns></returns>
        public string[] GetPortNames()
        {
            return new[] { this.ValidPortName, this.InvalidPortName };
        }
    }
}
