using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Boredbone.Utility.Extensions;
using Reactive.Bindings.Extensions;

namespace Terminal.Models.Serial
{
    public class ConnectionHistory : IDisposable
    {

        private ConnectionBase Connection { get; }

        private CompositeDisposable Disposables { get; }

        private List<string> HistoryList { get; }
        //public IReadOnlyList<string> History => this.HistoryList;
        private string LineBuffer { get; set; }


        /// <summary>
        /// データ送信による改行を含むデータ受信通知
        /// </summary>
        public IObservable<string> DataReceivedWithSendingLine { get; }

        /// <summary>
        /// 一行受信
        /// </summary>
        public IObservable<string> LineReceived
            => this.LineReceivedSubject.Buffer(this.DataReceivedWithSendingLine).SelectMany(y => y);
        //this.LineReceivedSubject.AsObservable();
        private Subject<string> LineReceivedSubject { get; }




        public ConnectionHistory(ConnectionBase connection)
        {
            this.Connection = connection;

            this.Disposables = new CompositeDisposable();

            this.HistoryList = new List<string>();
            this.LineBuffer = "";

            this.LineReceivedSubject = new Subject<string>().AddTo(this.Disposables);

            this.DataReceivedWithSendingLine = connection.DataReceived
                .Merge(connection.DataSent.Select(x => connection.ReceivingNewLine));

            //var buffer = "";

            //受信データを一行ごとに整形
            this.DataReceivedWithSendingLine
                .Buffer(this.DataReceivedWithSendingLine.Where(x => x.Contains(connection.ReceivingNewLine)))
                //.Delay(TimeSpan.FromMilliseconds(10))
                .Subscribe(list =>
                {
                    var fixedText = this.LineBuffer + list.Join();
                    var texts = fixedText.Split(connection.Splitter, StringSplitOptions.None);

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

            //this.LineReceived = this.LineReceivedSubject.Buffer(this.DataReceivedWithSendingLine).SelectMany(y => y);
        }


        public string History(int back)
        {

            if (back <= 0)
            {
                return this.LineBuffer;
            }

            var index = this.HistoryList.Count - back;

            return (index >= 0) ? this.HistoryList[index] : "";

        }

        public void Dispose()
        {
            this.Disposables.Dispose();
        }
    }
}
