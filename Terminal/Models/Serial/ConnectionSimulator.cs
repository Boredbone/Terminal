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
    public class ConnectionSimulator : ConnectionBase
    {
        private string ValidPortName = "Valid";
        private string InvalidPortName = "Invalid";

        protected override bool IsPortEnabled => this.IsOpen;


        public ConnectionSimulator()
        {
            //this.AddToDisposables(this.DataSent
            //    .Delay(TimeSpan.FromMilliseconds(2000))
            //    .Select(x => "echo:" + x + (x.Length > 0 ? "\n>" : ">"))
            //    //.Select(x => ">")//"echo:" + x + (x.Length > 0 ? "\n>" : ">"))
            //    .Subscribe(this.DataReceivedSubject));
            ////.AddTo(this.Disposables);

            this.AddToDisposables(this.DataSent
                .Subscribe(async str =>
                {
                    var command = str.Replace("\n", "");
                    string reply;

                    if (command.Length <= 0)
                    {
                        await Task.Delay(10);
                        reply = ">";
                    }
                    else
                    {
                        var cmds = command.Split(',');

                        if (cmds.Length >= 2 && cmds[0].Equals("delay"))
                        {
                            int num;
                            if (int.TryParse(cmds[1], out num))
                            {
                                await Task.Delay(num);
                            }
                        }
                        else
                        {
                            await Task.Delay(2000);
                        }
                        reply = "echo:" + command + "\n>";
                    }
                    this.DataReceivedSubject.OnNext(reply);

                }));

            this.AddToDisposables(this.IsOpenChanged.Where(y => y)
                .Delay(TimeSpan.FromMilliseconds(10))
                .Subscribe(y => this.DataReceivedSubject.OnNext(">")));
        }


        public override string[] GetPortNames()
        {
            return new[] { this.ValidPortName, this.InvalidPortName };
        }

        protected override void OnClosing()
        {
            //this.IsOpenProperty.Value = false;
        }

        protected override void OnOpening(string name)
        {
            if (name.Equals(this.ValidPortName))
            {
                //this.PortName = name;
                //this.IsOpenProperty.Value = true;
                //this.DataReceivedSubject.OnNext(">");
            }
            else
            {
                throw new ArgumentException("Invalid name");
            }
        }

        protected override void OnSending(string text)
        {
            //No operation
        }
    }
}
