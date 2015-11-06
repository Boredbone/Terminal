using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Boredbone.Utility.Extensions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Terminal.Macro.Api;

namespace Terminal.Models.Serial
{
    public class ConnectionSimulator : IConnection
    {
        public string SendingNewLine { get; set; } = "\r\n";
        public Encoding Encoding { get; set; } = Encoding.GetEncoding("Shift_JIS");
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;

        public string PortName { get; private set; }

        private string ValidPortName = "Valid";
        private string InvalidPortName = "Invalid";


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
        public IObservable<string> DataReceivedWithSendingLine { get; }

        /// <summary>
        /// 一行受信
        /// </summary>
        public IObservable<string> LineReceived => this.LineReceivedSubject.AsObservable();
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


        private string[] Splitter { get; set; }
        private string ReceivingNewLine { get; set; }
        private string IgnoredNewLine { get; set; }

        
        
        private CompositeDisposable Disposables { get; }



        public ConnectionSimulator()
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

            this.DataReceivedWithSendingLine = this.DataReceivedSubject
                .Merge(this.DataSentSubject.Select(x => this.ReceivingNewLine));

            var buffer = "";

            //受信データを一行ごとに整形
            this.DataReceivedWithSendingLine
                .Buffer(this.DataReceivedWithSendingLine.Where(x => x.Contains(this.ReceivingNewLine)))
                //.Delay(TimeSpan.FromMilliseconds(10))
                .Subscribe(list =>
                {
                    var fixedText = buffer + list.Join();
                    var texts = fixedText.Split(this.Splitter, StringSplitOptions.None);
                    
                    if (texts.Length > 0)
                    {
                        for (int i = 0; i < texts.Length - 1; i++)
                        {
                            this.LineReceivedSubject.OnNext(texts[i]);
                        }
                        buffer = texts[texts.Length - 1];
                    }

                })
                .AddTo(this.Disposables);

            this.DataSentSubject
                .Delay(TimeSpan.FromMilliseconds(2000))
                .Select(x => ">")//"echo:" + x + (x.Length > 0 ? "\n>" : ">"))
                .Subscribe(this.DataReceivedSubject)
                .AddTo(this.Disposables);

        }

        

        public void Open(string name)
        {
            //すでにポートを開いていたら例外
            if (this.IsOpen)
            {
                throw new InvalidOperationException("Already connected");
            }

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
        }


        public string[] GetPortNames()
        {
            return new[] { this.ValidPortName, this.InvalidPortName };
        }
    }
}
