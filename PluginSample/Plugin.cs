using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Macro.Api;

namespace PluginSample
{
    public class Plugin : IPlugin, INotifyPropertyChanged
    {

        private string _fieldWord;
        public string Word
        {
            get { return _fieldWord; }
            set
            {
                if (_fieldWord != value)
                {
                    _fieldWord = value;
                    RaisePropertyChanged(nameof(Word));
                }
            }
        }

        private int _fieldLoop;
        public int Loop
        {
            get { return _fieldLoop; }
            set
            {
                if (_fieldLoop != value)
                {
                    _fieldLoop = value;
                    RaisePropertyChanged(nameof(Loop));
                }
            }
        }

        internal IMacroPlayer Player { get; }


        internal Plugin(IMacroPlayer player)
        {
            this.Player = player;
        }

        /// <summary>
        /// プラグイン内部からの処理開始要求
        /// </summary>
        internal void Run()
        {
            this.Player.Start("Plubin Sample Work", this.Work);
        }

        /// <summary>
        /// 他のマクロからの処理開始要求
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync()
        {
            await this.Work(this.Player.Engine, this.Player.Plugins);
        }


        private async Task Work(IMacroEngine Macro, IPluginManager Plugins)
        {
            foreach (var c in Enumerable.Range(0, this.Loop))
            {
                await this.SendAndWait(Macro, this.Word, 50);
                Macro.Display(c.ToString());
            }
            Macro.Display("completed");
        }

        private async Task SendAndWait(IMacroEngine Macro, string text, int timems)
        {
            await Macro.SendLineAsync(text);
            await Macro.WaitAsync("\n>", ">> ?", "\n/>");
            await Macro.DelayAsync(timems);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
