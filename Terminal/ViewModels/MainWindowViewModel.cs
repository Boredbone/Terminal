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
    /// <summary>
    /// コマンド入出力画面のViewModel
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private int LogSize = 10000;

        private string[] splitter = new[] { "\n" };
        private string ignoredNewLine = "\r";

        private ApplicationCore Core { get; }
        private ConnectionBase Connection { get; }

        private string InputText { get; set; }
        private List<string> TextHistory { get; }

        public ObservableCollection<string> PortNames { get; }
        public ReactiveProperty<string> PortName { get; }
        public ReactiveProperty<bool> IsPortOpen { get; }

        public ObservableCollection<LogItem> Texts { get; }
        public ReactiveCollection<LogItem> LimitedTexts { get; }
        public ReadOnlyReactiveCollection<string> ReceivedTexts { get; }

        public ReactiveProperty<string> RequestedText { get; }
        public ReactiveProperty<int> TextHistoryIndex { get; }

        public ReactiveProperty<string> PauseText { get; }
        public ReactiveProperty<string> ConnectionText { get; }

        public ReactiveProperty<string> NoticeText { get; }
        public ReactiveProperty<bool> IsNoticeEnabled { get; }

        public ReactiveProperty<bool> IsMacroPlaying { get; }
        public ReactiveProperty<bool> IsMacroPausing { get; }

        //public ReactiveProperty<ScrollUnit> ScrollMode { get; }

        public ReactiveCommand GetPortNamesCommand { get; }
        public ReactiveCommand OpenPortCommand { get; }
        public ReactiveCommand ClosePortCommand { get; }
        public ReactiveCommand SendCommand { get; }
        public ReactiveCommand CopyCommand { get; }
        public ReactiveCommand MacroCommand { get; }
        public ReactiveCommand MacroCancelCommand { get; }
        public ReactiveCommand MacroPauseCommand { get; }
        public ReactiveCommand LaunchPluginCommand { get; }
        public ReactiveCommand ClearCommand { get; }
        public ReactiveCommand AboutCommand { get; }

        private Subject<bool> ScrollRequestSubject { get; }
        //private int scrollDelayTimeFast = 100;
        //private int scrollDelayTimeSlow = 1000;
        //private int scrollDelayTimeCurrent;

        public ReactiveProperty<bool> IsLogFollowing { get; }
        //private bool isLogFollowing = true;
        //private bool autoScroll = true;
        //private double autoScrollThreshold = 10;
        private double lastVerticalOffset = 0;

        public ListView TextsList { get; set; }
        public ScrollViewer ListScroller { get; set; }
        public Window View { get; set; }


        private Task ProcessTask { get; }
        private CancellationTokenSource CancellationTokenSource { get; }
        private EventWaitHandle WaitHandle { get; }
        private ConcurrentQueue<Action> ActionQueue { get; }


        public MainWindowViewModel()
        {
            this.Core = ((App)Application.Current).CoreData;

            this.ActionQueue = new ConcurrentQueue<Action>();
            this.WaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            this.IsLogFollowing = new ReactiveProperty<bool>(true).AddTo(this.Disposables);

            this.Texts = new ObservableCollection<LogItem>();
            var logUpdated = this.Texts.ObserveAddChanged();
            this.LimitedTexts = logUpdated
                .Where(y => this.IsLogFollowing.Value || y.LogType == LogTypes.Error || y.LogType == LogTypes.MacroMessage)
                .ToReactiveCollection().AddTo(this.Disposables);

            logUpdated.Subscribe(y =>
            {
                while (this.LimitedTexts.Count > LogSize)
                {
                    this.LimitedTexts.RemoveAtOnScheduler(0);
                }
            }).AddTo(this.Disposables);

            this.TextHistory = new List<string>();
            this.InputText = "";
            this.TextHistoryIndex = new ReactiveProperty<int>(0).AddTo(this.Disposables);

            //ユーザー入力テキスト
            this.RequestedText = this.TextHistoryIndex
                .Select(x => this.TextHistory.FromIndexOrDefault(x) ?? this.InputText)
                .ToReactiveProperty()
                .AddTo(this.Disposables);

            //this.ReceivedTexts = new ObservableCollection<string>();

            this.NoticeText = new ReactiveProperty<string>().AddTo(this.Disposables);
            this.IsNoticeEnabled = new ReactiveProperty<bool>(false).AddTo(this.Disposables);


            this.ScrollRequestSubject = new Subject<bool>().AddTo(this.Disposables);



            this.PortNames = new ObservableCollection<string>();
            this.PortName = new ReactiveProperty<string>("").AddTo(this.Disposables);


            this.Connection = this.Core.Connection;

            this.IsPortOpen = this.Connection.IsOpenChanged.ToReactiveProperty().AddTo(this.Disposables);


            //ポートOpen/Close時動作
            this.IsPortOpen.Skip(1).Subscribe(x =>
            {
                this.WriteNotice($"port {this.Connection.PortName} {(x ? "opened" : "closed")}",
                    false, LogTypes.Notice);
            })
            .AddTo(this.Disposables);


            //データ受信時の動作
            this.Connection
                .DataReceived
                .Subscribe(str => this.Write(str, false))
                .AddTo(this.Disposables);

            //データ送信時のエコー
            this.Connection
                .DataSent
                .Subscribe(str => this.Write(str, true))
                .AddTo(this.Disposables);

            //データが送信されなかった
            this.Connection
                .DataIgnored
                .Subscribe(str => this.WriteNotice(str, false, LogTypes.DisabledMessage))
                .AddTo(this.Disposables);


            //一行受信
            this.ReceivedTexts = this.Connection
                .LineReceived
                .ToReadOnlyReactiveCollection()
                .AddTo(this.Disposables);

                //.ObserveOnUIDispatcher()
                //.Subscribe(this.ReceivedTexts.Add)
                //.AddTo(this.Disposables);





            //ポートを開く
            this.OpenPortCommand = this.IsPortOpen
                .Select(x => !x)
                .ToReactiveCommand()
                .WithSubscribe(x =>
                {
                    try
                    {
                        this.Connection.Open(this.PortName.Value);
                    }
                    catch (Exception ex)
                    {
                        this.WriteNotice("Exception : " + ex.ToString(), false, LogTypes.Error);
                    }
                }, this.Disposables);



            //ポートを閉じる
            this.ClosePortCommand = this.IsPortOpen
                .ToReactiveCommand()
                .WithSubscribe(x => this.Connection.Close(), this.Disposables);


            this.ConnectionText = this.IsPortOpen
                .Select(y => y ? "Opened" : "Closed")
                .ToReactiveProperty()
                .AddTo(this.Disposables);

            //データ送信
            this.SendCommand = new ReactiveCommand()
                .WithSubscribe(x =>
                {
                    var text = this.RequestedText.Value;

                    this.InputText = "";

                    this.Connection.WriteLine(text);

                    if (text.Length > 0)
                    {
                        this.TextHistory.Add(text);
                        this.TextHistoryIndex.Value = this.TextHistory.Count;
                    }

                    this.ScrollToBottom(true);

                }, this.Disposables);



            //文字列コピー
            this.CopyCommand = new ReactiveCommand()
                .WithSubscribe(_ =>
                {
                    var items = this.TextsList.SelectedItems
                        .AsEnumerableWithSafeCast<LogItem>()
                        .OrderBy(x => x.Index)
                        .Select(x => x.Text)
                        .Join(Environment.NewLine);

                    Clipboard.SetDataObject(items.ToString(), true);
                    
                }, this.Disposables);

            this.ClearCommand = new ReactiveCommand()
                .WithSubscribe(y =>
                {
                    this.LimitedTexts.ClearOnScheduler();
                }, this.Disposables);
            

            //ポート名一覧を取得
            this.GetPortNamesCommand = this.IsPortOpen
                .Select(x => !x)
                .ToReactiveCommand()
                .WithSubscribe(_ =>
                {
                    var names = this.Connection.GetPortNames();
                    this.PortNames.Clear();
                    names.ForEach(x => this.PortNames.Add(x));
                }, this.Disposables);


            //プラグインの起動
            this.LaunchPluginCommand = new ReactiveCommand()
                .WithSubscribe(y => this.LaunchPlugin(), this.Disposables);

            this.AboutCommand = new ReactiveCommand()
                .WithSubscribe(y =>
                {
                    var window = new LicenseWindow();
                    
                    //表示
                    window.Show();
                    window.Activate();
                }, this.Disposables);

            //マクロ関連

            var player = this.Core.MacroPlayer;

            this.IsMacroPlaying = player.IsExecutingChanged
                .ObserveOnUIDispatcher()
                .ToReactiveProperty();
            this.IsMacroPausing = player.IsPausing
                .ObserveOnUIDispatcher()
                .ToReactiveProperty();


            this.PauseText = player.IsPausing
                .Select(y => y ? "Resume" : "Pause")
                .ToReactiveProperty()
                .AddTo(this.Disposables);

            //マクロメッセージ
            player.Message.Subscribe(y =>
            {
                if (y.Type == StatusType.Message)
                {
                    this.WriteNotice(y.Text, false, LogTypes.MacroMessage);
                }
                else// if (this.IsNoticeEnabled.Value)
                {
                    this.WriteNotice(y.Text, false, LogTypes.Notice);
                }
            })
            .AddTo(this.Disposables);

            //マクロエラー
            player.Error.Subscribe(y =>
            {
                this.WriteNotice(y, false, LogTypes.Error);
            })
            .AddTo(this.Disposables);

            //マクロ実行開始
            this.MacroCommand = player.IsExecutingChanged
                .Select(x => !x)
                .ObserveOnUIDispatcher()
                .ToReactiveCommand()
                .WithSubscribe(x => this.StartMacro(), this.Disposables);

            //マクロ終了
            this.MacroCancelCommand = player.IsExecutingChanged
                .ObserveOnUIDispatcher()
                .ToReactiveCommand()
                .WithSubscribe(x => player.Cancel(), this.Disposables);

            //マクロ一時停止・再開
            this.MacroPauseCommand = player.IsExecutingChanged
                .ObserveOnUIDispatcher()
                .ToReactiveCommand()
                .WithSubscribe(x => player.PauseOrResume(), this.Disposables);

            player.LogState
                .ObserveOnUIDispatcher()
                .Subscribe(y =>
                {
                    var prev = this.IsLogFollowing.Value;
                    this.IsLogFollowing.Value = y;

                    var length = this.Texts.Count - 1;
                    if (length >= 0)
                    {
                        var lastItem = this.Texts[length];
                        if (lastItem != this.LimitedTexts[this.LimitedTexts.Count - 1])
                        {
                            //this.LimitedTexts.AddOnScheduler(lastItem);
                        }
                    }

                    if (!prev && y)
                    {
                        this.ScrollToBottom(true);
                    }
                })
                .AddTo(this.Disposables);



            //メッセージ表示タスクのキャンセル
            this.CancellationTokenSource = new CancellationTokenSource().AddTo(this.Disposables);

            //メッセージ表示リクエストを処理
            this.ProcessTask = Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        //次の要求が来るまで待機
                        this.WaitHandle.WaitOne(-1);
                        

                        //キューにリクエストがあれば実行
                        Action request;
                        while (this.ActionQueue.TryDequeue(out request))
                        {
                            this.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                            if (request == null)
                            {
                                continue;
                            }
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                request();
                            });//,System.Windows.Threading.DispatcherPriority.Render);
                        }
                    }
                }
                catch
                {
                    //throw;
                }
            }, this.CancellationTokenSource.Token);



            


            //リストに空アイテムを追加
            this.FeedLine();

            //ポート一覧を取得
            this.GetPortNamesCommand.Execute();
            this.PortName.Value = this.PortNames.FirstOrDefault();

            if (this.PortNames.Count == 1)
            {
                this.OpenPortCommand.Execute();
            }

            this.PortName.Skip(1).Subscribe(name =>
            {
                if (!this.IsPortOpen.Value && name!=null && name.Length > 0)
                {
                    this.OpenPortCommand.Execute();
                }
            })
            .AddTo(this.Disposables);

        }

        /// <summary>
        /// 末尾までスクロール
        /// </summary>
        /// <param name="force"></param>
        private void ScrollToBottom(bool force)
        {
            //return;
            if (this.ListScroller == null && this.TextsList != null)
            {
                this.ListScroller = this.TextsList.Descendants<ScrollViewer>().FirstOrDefault();
            }

            if (this.ListScroller != null)
            {

                
                if (this.IsLogFollowing.Value)
                {
                    var nearBottom = this.ListScroller.VerticalOffset > this.lastVerticalOffset - 2;
                    //var nearBottom = this.ListScroller.VerticalOffset > this.ListScroller.ScrollableHeight - 2;

                    //if (!nearBottom)
                    //{
                    //    //this.autoScroll = false;
                    //    //this.autoScrollThreshold = 10;
                    //}
                    //else if (this.ListScroller.ScrollableHeight - this.ListScroller.VerticalOffset
                    //    < this.autoScrollThreshold)
                    //{
                    //    this.autoScroll = true;
                    //    //this.autoScrollThreshold = this.ListScroller.ActualHeight * 0.4;
                    //}
                    //this.autoScroll = nearBottom;

                    //if (force || this.ListScroller.VerticalOffset > this.ListScroller.ScrollableHeight - 2)
                    if (force || nearBottom)//this.autoScroll)//(nearBottom && this.autoScroll)
                    {
                        this.lastVerticalOffset = this.ListScroller.VerticalOffset;
                        this.ScrollRequestSubject.OnNext(true);
                        this.ListScroller.ScrollToBottom();
                    }
                }
            }
        }

        /// <summary>
        /// コンソールに文字列を表示
        /// </summary>
        /// <param name="text"></param>
        private void Write(string text, bool feed)
        {
            this.ActionQueue.Enqueue(() =>
            {
                var fixedText = text.Replace(this.ignoredNewLine, "");
                var texts = fixedText.Split(splitter, StringSplitOptions.None);

                var count = this.Texts.Count;
                this.Texts[count - 1].Text = this.Texts[count - 1].Text + texts.First();

                texts.Skip(1).ForEach(x => this.AddLine(x, LogTypes.Normal));
                if (feed)
                {
                    this.FeedLine();
                }
                this.ScrollToBottom(false);
            });
            this.WaitHandle.Set();
        }

        /// <summary>
        /// 通知文字列を表示
        /// </summary>
        /// <param name="text"></param>
        /// <param name="forceScroll"></param>
        /// <param name="type"></param>
        private void WriteNotice(string text, bool forceScroll, LogTypes type)
        {
            if (type == LogTypes.Notice)
            {
                Application.Current.Dispatcher.Invoke
                    (() => this.NoticeText.Value = text);

                if (!this.IsNoticeEnabled.Value)
                {
                    return;
                }
            }

            this.ActionQueue.Enqueue(() =>
            {
                var fixedText = text.Replace(this.ignoredNewLine, "");
                var texts = fixedText.Split(splitter, StringSplitOptions.None);

                int index = 0;

                foreach (var item in texts)
                {
                    if (index == 0)
                    {
                        //最後の行が空白なら置き換え
                        var count = this.Texts.Count;
                        if (this.Texts.ContainsIndex(count - 1) && this.Texts[count - 1].Text.Length <= 0)
                        {
                            this.Texts[count - 1].Text = item;
                            this.Texts[count - 1].LogType = type;
                            //this.Texts.RemoveAt(count - 1);
                        }
                        else
                        {
                            this.AddLine(item, type);
                        }
                    }
                    else
                    {
                        this.AddLine(item, type);
                    }
                    index++;
                }

                //texts.ForEach(x => this.AddLine(x, type));

                this.FeedLine();
                this.ScrollToBottom(forceScroll);

            });
            this.WaitHandle.Set();
        }


        /// <summary>
        /// 新しい行を追加して文字列を記入
        /// </summary>
        /// <param name="text"></param>
        private void AddLine(string text, LogTypes type)
        {
            this.Texts.Add(new LogItem()
            {
                Text = text,
                Index = this.Texts.Count,
                LogType = type
            });
        }

        /// <summary>
        /// 新しい空の行を追加
        /// </summary>
        private void FeedLine()
        {
            this.AddLine("", LogTypes.Normal);
        }
        


        /// <summary>
        /// 入力履歴を進める
        /// </summary>
        public void IncrementHistoryIndex()
        {
            if (this.InputText.Length > 0)
            {

                var index = this.TextHistoryIndex.Value + 1;

                while (true)
                {
                    if (index == this.TextHistory.Count)
                    {
                        this.TextHistoryIndex.Value = index;
                        return;
                    }
                    if (!this.TextHistory.ContainsIndex(index))
                    {
                        return;
                    }
                    if (this.TextHistory[index].StartsWith(this.InputText))
                    {
                        this.TextHistoryIndex.Value = index;
                        return;
                    }
                    index++;
                }
            }
            else if (this.TextHistoryIndex.Value < this.TextHistory.Count)
            {
                this.TextHistoryIndex.Value++;
            }
        }

        /// <summary>
        /// 入力履歴をさかのぼる
        /// </summary>
        public void DecrementHistoryIndex()
        {
            if (this.TextHistoryIndex.Value == this.TextHistory.Count)
            {
                this.InputText = this.RequestedText.Value;
            }

            if (this.InputText.Length > 0)
            {

                var index = this.TextHistoryIndex.Value - 1;

                while (true)
                {
                    if (!this.TextHistory.ContainsIndex(index))
                    {
                        return;
                    }
                    if (this.TextHistory[index].StartsWith(this.InputText))
                    {
                        this.TextHistoryIndex.Value = index;
                        return;
                    }
                    index--;
                }
            }
            else if (this.TextHistoryIndex.Value > 0)
            {
                this.TextHistoryIndex.Value--;
            }
        }

        /// <summary>
        /// ファイルを選択してマクロを実行
        /// </summary>
        private void StartMacro()
        {

            var dialog = new OpenFileDialog();
            //dialog.Title = "Open File";
            dialog.Filter = "C# code(*.cs)|*.cs|All Files(*.*)|*.*";
            if (dialog.ShowDialog() != true)
            {
                return;
            }


            var path = dialog.FileName;

            //CodeBlock[] blocks;
            var text = "";

            try
            {
                text = new TextLoader().Load(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.GetType().FullName + ": " + e.Message);
                return;
            }

            var blocks = new CodeReader().GetBlocks(text);

            var name = "";
            var code = "";

            if (blocks == null || blocks.Length <= 0)
            {
                name = System.IO.Path.GetFileName(path);
                code = text;
            }
            else
            {
                var index = 0;
                if (blocks.Length > 1)
                {
                    var listDialog = new SelectorWindow();
                    listDialog.Owner = this.View;
                    listDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    blocks.ForEach(y => listDialog.List.Add(y));

                    if (listDialog.ShowDialog() == true)
                    {
                        index = listDialog.Index;
                    }
                    else
                    {
                        return;
                    }

                }
                name = blocks[index].Name;
                code = blocks[index].Code;
            }




            var player = this.Core.MacroPlayer;

            //this.WriteNotice("Macro Loading", false, LogTypes.Notice);

            //var sc = new DelegateMacro("Macro01", null);
            var sc = new ScriptMacro(name, code);

            try
            {
                player.Start(sc);
            }
            catch (Exception e)
            {
                this.WriteNotice(e.ToString(), false, LogTypes.Error);
            }
        }

        /// <summary>
        /// プラグインを選択して起動
        /// </summary>
        private void LaunchPlugin()
        {
            var plugins = this.Core.Plugins;

            var listDialog = new SelectorWindow();
            listDialog.Owner = this.View;
            listDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            plugins.ForEach(y => listDialog.List.Add(y));

            if (listDialog.ShowDialog() == true)
            {
                var index = listDialog.Index;

                var plugin = plugins.FromIndexOrDefault(index);
                if (plugin != null)
                {
                    this.Core.LaunchPluginUI(plugin);
                }
            }
        }

    }
}
