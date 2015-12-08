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
    /// <summary>
    /// SerialPortの管理
    /// </summary>
    public class SerialConnection : ConnectionBase
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

        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;

        protected override bool IsPortEnabled => this.Port != null;

        private CompositeDisposable ConnectionDisposables { get; }

        public SerialConnection()
        {
            this.ConnectionDisposables = new CompositeDisposable();
            this.AddToDisposables(this.ConnectionDisposables);
        }

        //public override string[] GetPortNames()
        //{
        //    try
        //    {
        //        return SerialPort.GetPortNames();
        //    }
        //    catch//(Exception e)
        //    {
        //        return new string[0];
        //        //return new string[] { e.ToString() };
        //    }
        //}
        public override void RefreshPortNames()
        {
            this.portNames.Clear();
            try
            {
                var names = SerialPort.GetPortNames();
                foreach (var item in names)
                {
                    this.portNames.Add(item);
                }
            }
            catch//(Exception e)
            {
                //return new string[] { e.ToString() };
            }

        }

        protected override void OnClosing()
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
            //this.IsOpenProperty.Value = false;
            this.ConnectionDisposables.Clear();
        }

        protected override void OnOpening(string name)
        {

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
                //this.PortName = name;
                //this.IsOpenProperty.Value = true;
            }
            catch
            {
                //ポートオープンに失敗
                port.Dispose();
                this.Port = null;
                throw;
            }
        }

        protected override void OnSending(string text)
        {
            this.Port.WriteLine(text);
        }
    }
}
