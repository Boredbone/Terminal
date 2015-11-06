using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Macro.Api
{
    public interface IConnection : IDisposable
    {
        LineCodes LineCode { get; set; }
        Encoding Encoding { get; set; }
        string SendingNewLine { get; set; }
        int BaudRate { get; set; }
        int DataBits { get; set; }
        string PortName { get; }
        bool IsOpen { get; }

        /// <summary>
        /// ポート変更通知
        /// </summary>
        IObservable<bool> Portchanged { get; }

        /// <summary>
        /// ポートOpen/Close通知
        /// </summary>
        IObservable<bool> IsOpenChanged { get; }

        /// <summary>
        /// データ受信通知
        /// </summary>
        IObservable<string> DataReceived { get; }

        /// <summary>
        /// データ送信による改行を含むデータ受信通知
        /// </summary>
        IObservable<string> DataReceivedWithSendingLine { get; }

        /// <summary>
        /// 一行受信
        /// </summary>
        IObservable<string> LineReceived { get; }

        /// <summary>
        /// ローカルエコー
        /// </summary>
        IObservable<string> DataSent { get; }

        /// <summary>
        /// データ送信失敗
        /// </summary>
        IObservable<string> DataIgnored { get; }



        /// <summary>
        /// ポートを開く
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        void Open(string name);

        /// <summary>
        /// 送信
        /// </summary>
        /// <param name="text"></param>
        void WriteLine(string text);

        /// <summary>
        /// ポートを閉じる
        /// </summary>
        void Close();

        /// <summary>
        /// ポート名一覧取得
        /// </summary>
        /// <returns></returns>
        string[] GetPortNames();
    }
}
