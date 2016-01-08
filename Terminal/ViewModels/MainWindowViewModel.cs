using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        private int LogSizeMax = 4000;
        private int LogSizeCompressed = 2000;

        private string[] splitter = new[] { "\n" };
        private string ignoredNewLine = "\r";

        private ApplicationCore Core { get; }
        private ConnectionBase Connection { get; }

        private string InputText { get; set; }
        private List<string> TextHistory { get; }

        public ReadOnlyReactiveCollection<string> PortNames { get; }
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
        private bool isAutoScrollEnabled = true;
        //private double autoScrollThreshold = 10;
        private double lastVerticalOffset = 0;

        //private Subject<bool> ScrollSubject { get; }

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


            if (false)
            {
                this.LimitedTexts = new ReactiveCollection<LogItem>().AddTo(this.Disposables);
                var margin = 8;
                this.LimitedTexts
                    .AddRangeOnScheduler(Enumerable.Range(0, margin)
                        .Select(_ => new LogItem() { Index = -1 }));

                logUpdated
                    .Where(y => this.IsLogFollowing.Value
                        || y.LogType == LogTypes.Error
                        || y.LogType == LogTypes.MacroMessage)
                    .Subscribe(y => this.LimitedTexts
                        .InsertOnScheduler(Math.Max(0, this.LimitedTexts.Count - margin), y))
                    .AddTo(this.Disposables);
            }
            else
            {
                this.LimitedTexts = logUpdated
                    .Where(y => this.IsLogFollowing.Value
                        || y.LogType == LogTypes.Error
                        || y.LogType == LogTypes.MacroMessage)
                    .ToReactiveCollection().AddTo(this.Disposables);
            }

            logUpdated.Subscribe(y =>
            {
                var over = this.LimitedTexts.Count - this.LogSizeCompressed;

                if (this.LimitedTexts.Count > this.LogSizeMax)
                {
                    var data = this.LimitedTexts.Skip(over).ToArray();
                    this.LimitedTexts.ClearOnScheduler();
                    this.LimitedTexts.AddRangeOnScheduler(data);

                    Debug.WriteLine("log over");

                    //if (this.ListScroller != null)
                    //{
                    //    var scrollableHeight = this.ListScroller.ScrollableHeight;
                    //    if (this.lastVerticalOffset > scrollableHeight)
                    //    {
                    //        this.lastVerticalOffset = scrollableHeight - 20;
                    //    }
                    //}
                    //while (this.LimitedTexts.Count > this.LogSizeCompressed)
                    //{
                    //    this.LimitedTexts.RemoveAtOnScheduler(0);
                    //}
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


            // ログ画面スクロール要求
            this.ScrollRequestSubject = new Subject<bool>().AddTo(this.Disposables);


            var down = this.ScrollRequestSubject
                .DownSample(TimeSpan.FromMilliseconds(1000));

            var ct = this.ScrollRequestSubject
                .Buffer(TimeSpan.FromMilliseconds(500))//(down)
                .Select(y => y.Count)
                .ToReactiveProperty()
                .AddTo(this.Disposables);

            var quick = this.ScrollRequestSubject
                .Where(y => ct.Value < 20);
            //var quick = this.ScrollRequestSubject
            //    .CombineLatest(ct, (Value, Count) => new { Value, Count })
            //    .Where(y =>
            //    {
            //        Debug.WriteLine($"{y.Value}, {y.Count}");
            //        return y.Count < 20;
            //    })
            //    .Select(y => y.Value);

            this.ScrollRequestSubject
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Merge(down)
                .Merge(quick)
                //.Throttle(TimeSpan.FromMilliseconds(10))
                .Buffer(TimeSpan.FromMilliseconds(10))
                .Where(y => y.Count > 0)
                .Select(y => y.Any(b => b))//.Last())
                .ObserveOnUIDispatcher()
                .Subscribe(y => this.ScrollToBottomMain(y))
                .AddTo(this.Disposables);

            //this.ScrollRequestSubject
            //    .Throttle(TimeSpan.FromMilliseconds(500))
            //    .Merge(this.ScrollRequestSubject
            //        .DownSample(TimeSpan.FromMilliseconds(1000)))
            //    .Merge(this.ScrollRequestSubject
            //        .Buffer(TimeSpan.FromMilliseconds(500))
            //        .Where(y => y.Count <= 2 && y.Count > 0)
            //        .Select(y => y.Last()))
            //    .ObserveOnUIDispatcher()
            //    .Subscribe(y => this.ScrollToBottomMain(y))
            //    .AddTo(this.Disposables);




            this.Connection = this.Core.Connection;

            this.PortNames = this.Connection.PortNames.ToReadOnlyReactiveCollection();

            this.PortName = this.Connection.IsOpenChanged
                .Where(y => y)
                .Select(y => this.Connection.PortName)
                .ToReactiveProperty(this.Connection.PortName)
                .AddTo(this.Disposables);
            // new ReactiveProperty<string>("").AddTo(this.Disposables);



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

                    if (text != null && text.Length > 0)
                    {
                        if (this.TextHistory.Count < 1
                            || !this.TextHistory[this.TextHistory.Count - 1].Equals(text))
                        {
                            this.TextHistory.Add(text);
                        }

                        this.TextHistoryIndex.Value = this.TextHistory.Count;
                    }

                    if (this.RequestedText.Value.Length > 0)
                    {
                        Debug.WriteLine(this.RequestedText.Value);
                    }

                    this.RequestedText.Value = "";

                    this.ScrollToBottom(true);

                }, this.Disposables);



            //文字列コピー
            this.CopyCommand = new ReactiveCommand()
                .WithSubscribe(_ =>
                {
                    var items = this.TextsList.SelectedItems
                        .AsEnumerableWithSafeCast<LogItem>()
                        .Where(x => x.Index >= 0)
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
                    this.Connection.RefreshPortNames();
                    //if (this.PortNames.Count > 0 && !this.Connection.IsOpen)
                    //{
                    //    this.PortName.Value = this.PortNames.First();
                    //}

                }, this.Disposables);

            //.WithSubscribe(_ =>
            //{
            //    var names = this.Connection.GetPortNames();
            //    this.PortNames.Clear();
            //    names.ForEach(x => this.PortNames.Add(x));
            //}, this.Disposables);


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

                    //var length = this.Texts.Count - 1;
                    //if (length >= 0)
                    //{
                    //    var lastItem = this.Texts[length];
                    //    if (lastItem != this.LimitedTexts[this.LimitedTexts.Count - 1])
                    //    {
                    //        //this.LimitedTexts.AddOnScheduler(lastItem);
                    //    }
                    //}

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


            this.PortName.Skip(1).Subscribe(name =>
            {
                if (!this.IsPortOpen.Value && name != null && name.Length > 0)
                {
                    this.OpenPortCommand.Execute();
                }
            })
            .AddTo(this.Disposables);

        }


        private void ScrollToBottom(bool force)
        {
            if (this.ListScroller != null
                && this.ListScroller.VerticalOffset > this.ListScroller.ScrollableHeight - 10)
            {
                force = true;
            }
            this.ScrollRequestSubject.OnNext(force);
        }

        /// <summary>
        /// 末尾までスクロール
        /// </summary>
        /// <param name="force"></param>
        private void ScrollToBottomMain(bool force)
        {
            //return;
            if (this.ListScroller == null && this.TextsList != null)
            {
                this.ListScroller = this.TextsList.Descendants<ScrollViewer>().FirstOrDefault();

                //if (this.ListScroller != null)
                //{
                //    var tx = this.ListScroller.Content as FrameworkElement;
                //    if (tx != null)
                //    {
                //        tx.Margin = new Thickness(0, 0, 0, 64);
                //    }
                //    //var tx = this.ListScroller.Margin = new Thickness(0, 0, 0, 64);
                //}
            }

            if (this.ListScroller != null)
            {


                if (this.IsLogFollowing.Value)
                {

                    var scrollableHeight = this.ListScroller.ScrollableHeight;
                    var offset = this.ListScroller.VerticalOffset;
                    var nearBottom = offset > this.lastVerticalOffset - 10;
                    //var nearBottom = this.ListScroller.VerticalOffset > scrollableHeight - 10;

                    if (this.lastVerticalOffset > scrollableHeight && this.isAutoScrollEnabled)
                    {
                        this.lastVerticalOffset = offset;// scrollableHeight - 20;
                    }

                    if (offset < this.lastVerticalOffset - 10)
                    {
                        nearBottom = false;
                        this.isAutoScrollEnabled = false;
                    }


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

                    if (force || nearBottom)
                    {
                        if (!this.isAutoScrollEnabled)
                        {
                            //this.lastVerticalOffset = this.ListScroller.VerticalOffset;
                            this.isAutoScrollEnabled = true;
                        }
                    }

                    //if (force || this.ListScroller.VerticalOffset > this.ListScroller.ScrollableHeight - 2)
                    if (this.isAutoScrollEnabled && offset < scrollableHeight)//this.autoScroll)//(nearBottom && this.autoScroll)
                    {
                        this.lastVerticalOffset = scrollableHeight;// this.ListScroller.VerticalOffset;
                        //this.ScrollRequestSubject.OnNext(true);
                        this.ListScroller.ScrollToBottom();
                        Debug.WriteLine(offset.ToString());
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
                var added = this.Texts[count - 1].Text + texts.First();
                this.Texts[count - 1].Text = added;

                texts.Skip(1).ForEach(x => this.AddLine(x, LogTypes.Normal));
                if (feed)
                {
                    this.FeedLine();
                }

                if (feed || texts.Length > 1 || added.Length > 20)
                {
                    this.ScrollToBottom(false);
                }
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
            if (this.Texts.Count > 0)
            {
                this.Texts[this.Texts.Count - 1].IsLast = false;
            }

            this.Texts.Add(new LogItem()
            {
                Text = text,
                Index = this.Texts.Count,
                LogType = type,
                IsLast = true,
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
