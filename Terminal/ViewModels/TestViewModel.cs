using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Boredbone.Utility;
using Boredbone.Utility.Extensions;
using Boredbone.XamlTools.Extensions;
using Boredbone.XamlTools.ViewModel;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Terminal.Macro.Api;
using Terminal.Models;
using Terminal.Models.Macro;
using Terminal.Models.Serial;
using Terminal.Views;

namespace Terminal.ViewModels
{
    public class TestViewModel : ViewModelBase
    {

        //private string[] splitter = new[] { "\n" };
        //private string ignoredNewLine = "\r";
        //
        //private ApplicationCore Core { get; }
        //private ConnectionBase Connection { get; }
        //
        //private string InputText { get; set; }
        //private List<string> TextHistory { get; }

        //public ListView TextsList { get; set; }
        //public ScrollViewer ListScroller { get; set; }
        //public Window View { get; set; }


        //public ReactiveCommand SendCommand { get; }
        //public ReactiveCommand CopyCommand { get; }
        /*
        public ObservableCollection<string> PortNames { get; }
        public ReactiveProperty<string> PortName { get; }
        public ReactiveProperty<bool> IsPortOpen { get; }

        public ObservableCollection<LogItem> Texts { get; }
        public ObservableCollection<string> ReceivedTexts { get; }

        public ReactiveProperty<string> RequestedText { get; }
        public ReactiveProperty<int> TextHistoryIndex { get; }

        public ReactiveProperty<string> PauseText { get; }

        public ReactiveProperty<bool> IsNoticeEnabled { get; }

        public ReactiveCommand GetPortNamesCommand { get; }
        public ReactiveCommand OpenPortCommand { get; }
        public ReactiveCommand ClosePortCommand { get; }
        public ReactiveCommand SendCommand { get; }
        public ReactiveCommand CopyCommand { get; }
        public ReactiveCommand MacroCommand { get; }
        public ReactiveCommand MacroCancelCommand { get; }
        public ReactiveCommand MacroPauseCommand { get; }



        private Task ProcessTask { get; }
        private CancellationTokenSource CancellationTokenSource { get; }
        private EventWaitHandle WaitHandle { get; }
        private ConcurrentQueue<Action> ActionQueue { get; }


    */
        /// <summary>
        /// 入力履歴を進める
        /// </summary>
        public void IncrementHistoryIndex()
        {
        }

        /// <summary>
        /// 入力履歴をさかのぼる
        /// </summary>
        public void DecrementHistoryIndex()
        {
        }
    }
}
