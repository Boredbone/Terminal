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
    /// <summary>
    /// SerialPortの管理
    /// </summary>
    public class SerialConnection : IConnection
    {
        private SerialPort _fieldPort;
        private SerialPort Port
        {
            get { return _fieldPort; }
            set
            {
                if (_fieldPort != value)
                {
                    _fieldPort?.Dispose();
                    _fieldPort = value;
                    if (this.PortChangedSubject.HasObservers)
                    {
                        this.PortChangedSubject.OnNext(value != null);
                    }
                }
            }
        }

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


        public string SendingNewLine { get; set; } = "\r\n";
        public Encoding Encoding { get; set; } = Encoding.GetEncoding("Shift_JIS");
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;

        public string PortName { get; private set; }

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

        

        //private Subject<string> InputNewLineSubject { get; }
        
        private CompositeDisposable ConnectionDisposables { get; }
        private CompositeDisposable Disposables { get; }



        public SerialConnection()
        {
            this.Disposables = new CompositeDisposable();

            this.ConnectionDisposables = new CompositeDisposable().AddTo(this.Disposables);

            this.PortName = "";

            this.PortChangedSubject = new Subject<bool>().AddTo(this.Disposables);
            this.DataReceivedSubject = new Subject<string>().AddTo(this.Disposables);
            this.DataIgnoredSubject = new Subject<string>().AddTo(this.Disposables);
            this.LineReceivedSubject = new Subject<string>().AddTo(this.Disposables);
            this.DataSentSubject = new Subject<string>().AddTo(this.Disposables);
            //this.InputNewLineSubject = new Subject<string>().AddTo(this.Disposables);
            
            this.IsOpenProperty = new ReactiveProperty<bool>(false).AddTo(this.Disposables);

            this.LineCode = LineCodes.Lf;

            this.DataReceivedWithSendingLine = this.DataReceivedSubject
                .Merge(this.DataSentSubject.Select(x => this.ReceivingNewLine));

            var buffer = "";
            
            //受信データを一行ごとに整形
            this.DataReceivedWithSendingLine
                .Buffer(this.DataReceivedWithSendingLine.Where(x => x.Contains(this.ReceivingNewLine)))
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

        }


        /// <summary>
        /// シリアルポートを開く
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public void Open(string name)
        {
            //すでにポートを開いていたら例外
            if (this.Port != null)
            {
                throw new InvalidOperationException("Already connected");
            }

            var port = new SerialPort(name, this.BaudRate, Parity.None, this.DataBits, StopBits.One);
            
            this.ConnectionDisposables.Clear();
            
            try
            {
                port.Open();
                port.DtrEnable = true;
                port.RtsEnable = true;
                port.Encoding = this.Encoding;
                port.NewLine = this.SendingNewLine;
                //port.ReadBufferSize = 1024;

                //データ受信時の動作を登録
                Observable.FromEvent<SerialDataReceivedEventHandler, SerialDataReceivedEventArgs>(
                    h => (s, e) => h(e),
                    h => port.DataReceived += h,
                    h => port.DataReceived -= h)
                    .Select(x =>
                    {
                        //var pt1 = (SerialPort)pt;
                        //var buf = new byte[1024];
                        //var len = port.Read(buf, 0, 1024);
                        //var s = Encoding.GetEncoding("Shift_JIS").GetString(buf, 0, len);
                        //return s;
                        return port.ReadExisting()
                            .Replace("\0", "")
                            .Replace(this.IgnoredNewLine, "");
                    })
                    .Subscribe(this.DataReceivedSubject)
                    .AddTo(this.ConnectionDisposables);

                this.Port = port;
                this.PortName = name;
                this.IsOpenProperty.Value = true;
            }
            catch
            {
                //ポートオープンに失敗
                port.Dispose();
                this.Port = null;
                throw;
            }
        }

        /// <summary>
        /// 送信
        /// </summary>
        /// <param name="text"></param>
        public void WriteLine(string text)
        {
            if (this.Port != null)
            {
                this.Port.WriteLine(text);
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
            if (this.Port != null)
            {
                if (this.Port.IsOpen)
                {
                    this.Port.Close();
                }
                this.Port.Dispose();
                this.Port = null;
            }
            //this.PortName = "";
            this.IsOpenProperty.Value = false;
            this.ConnectionDisposables.Clear();
        }

        /// <summary>
        /// ポート名一覧取得
        /// </summary>
        /// <returns></returns>
        public string[] GetPortNames()
        {
            try
            {
                return SerialPort.GetPortNames();
            }
            catch//(Exception e)
            {
                return new string[0];
                //return new string[] { e.ToString() };
            }
        }
    }
}
