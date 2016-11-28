using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
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
using Boredbone.Utility.Notification;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Terminal.Models;
using Terminal.Models.Macro;
using Terminal.Models.Serial;
using Terminal.Views;
using WpfTools.Models;

namespace Terminal.ViewModels
{
    /// <summary>
    /// コマンド入出力画面のViewModel
    /// </summary>
    public class MainWindowViewModel : DisposableBase
    {
        private ApplicationCore Core { get; }

        private string InputText { get; set; }
        private List<string> TextHistory { get; }

        public ReadOnlyReactiveCollection<string> PortNames { get; }
        public ReactiveProperty<string> PortName { get; }
        public ReactiveProperty<bool> IsPortOpen { get; }

        public ReactiveProperty<string> RequestedText { get; }
        public ReactiveProperty<int> TextHistoryIndex { get; }

        public ReactiveProperty<string> PauseText { get; }
        public ReactiveProperty<string> ConnectionText { get; }

        public ReactiveProperty<string> NoticeText { get; }
        public ReactiveProperty<bool> IsNoticeEnabled { get; }

        public ReactiveProperty<bool> IsMacroPlaying { get; }
        public ReactiveProperty<bool> IsMacroPausing { get; }


        public ReactiveCommand GetPortNamesCommand { get; }
        public ReactiveCommand OpenPortCommand { get; }
        public ReactiveCommand ClosePortCommand { get; }
        public ReactiveCommand SendCommand { get; }

        public ReactiveCommand MacroCommand { get; }
        public ReactiveCommand MacroCancelCommand { get; }
        public ReactiveCommand MacroPauseCommand { get; }
        public ReactiveCommand LaunchPluginCommand { get; }
        public ReactiveCommand ClearCommand { get; }
        public ReactiveCommand AboutCommand { get; }
        public ReactiveCommand ConnectionConfigCommand { get; }


        public ReactiveCommand IncrementCommand { get; }
        public ReactiveCommand DecrementCommand { get; }

        public ReactiveProperty<bool> IsLogFollowing { get; }

        public AppendableTextController AppendableTextController { get; }

        private Subject<RequestContainer> ActionQueueSubject { get; }

        public Window View { get; set; }

        private bool textEdited = false;


        public MainWindowViewModel()
        {
            this.Core = ((App)Application.Current).CoreData;


            this.AppendableTextController = new AppendableTextController().AddTo(this.Disposables);

            this.IsLogFollowing = new ReactiveProperty<bool>(true).AddTo(this.Disposables);

            this.TextHistory = new List<string>();
            this.InputText = "";
            this.TextHistoryIndex = new ReactiveProperty<int>(0).AddTo(this.Disposables);

            //ユーザー入力テキスト
            this.RequestedText = this.TextHistoryIndex
                .Do(_ => this.textEdited = false)
                .Select(x => this.TextHistory.FromIndexOrDefault(x) ?? this.InputText)
                .ToReactiveProperty()
                .AddTo(this.Disposables);


            this.NoticeText = new ReactiveProperty<string>().AddTo(this.Disposables);
            this.IsNoticeEnabled = new ReactiveProperty<bool>(false).AddTo(this.Disposables);




            var connection = this.Core.Connection;

            this.PortNames = connection.PortNames.ToReadOnlyReactiveCollection();

            this.PortName = connection.IsOpenChanged
                .Where(y => y)
                .Select(y => connection.PortName)
                .ToReactiveProperty(connection.PortName)
                .AddTo(this.Disposables);


            this.IsPortOpen = connection.IsOpenChanged.ToReactiveProperty().AddTo(this.Disposables);


            //ポートOpen/Close時動作
            this.IsPortOpen.Skip(1).Subscribe(x =>
            {
                this.WriteNotice($"port {connection.PortName} {(x ? "opened" : "closed")}",
                    false, LogTypes.Notice);
            })
            .AddTo(this.Disposables);


            //データ受信時の動作
            connection
                .DataReceived
                .Subscribe(str => this.Write(str, false))
                .AddTo(this.Disposables);


            var isFeedAfterSend = !this.Core.NoFeedAfterSend;

            //データ送信時のエコー
            connection
                .DataSent
                .Subscribe(str => this.Write(str, isFeedAfterSend, true))
                .AddTo(this.Disposables);

            //データが送信されなかった
            connection
                .DataIgnored
                .Subscribe(str => this.WriteNotice(str, false, LogTypes.DisabledMessage))
                .AddTo(this.Disposables);


            //ポートを開く
            this.OpenPortCommand = this.IsPortOpen
                .Select(x => !x)
                .ToReactiveCommand()
                .WithSubscribe(x =>
                {
                    try
                    {
                        connection.Open(this.PortName.Value);
                    }
                    catch (Exception ex)
                    {
                        this.WriteNotice
                            ("Exception : " + ex.GetType().FullName + ": " + ex.Message,
                            false, LogTypes.Error);
                    }
                }, this.Disposables);



            //ポートを閉じる
            this.ClosePortCommand = this.IsPortOpen
                .ToReactiveCommand()
                .WithSubscribe(x => connection.Close(), this.Disposables);


            this.ConnectionText = this.IsPortOpen
                .Select(y => y ? "Open" : "Closed")
                .ToReactiveProperty()
                .AddTo(this.Disposables);

            //データ送信
            this.SendCommand = new ReactiveCommand()
                .WithSubscribe(x =>
                {
                    var text = this.RequestedText.Value;

                    this.InputText = "";

                    connection.WriteLine(text);

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

                    this.ScrollToBottom();

                }, this.Disposables);



            // ログのクリア
            this.ClearCommand = new ReactiveCommand()
                .WithSubscribe(y => this.AppendableTextController.Clear(), this.Disposables);

            // 入力履歴操作
            this.IncrementCommand = new ReactiveCommand()
                .WithSubscribe(_ => this.IncrementHistoryIndex(), this.Disposables);
            this.DecrementCommand = new ReactiveCommand()
                .WithSubscribe(_ => this.DecrementHistoryIndex(), this.Disposables);


            //ポート名一覧を取得
            this.GetPortNamesCommand = this.IsPortOpen
                .Select(x => !x)
                .ToReactiveCommand()
                .WithSubscribe(_ => connection.RefreshPortNames(), this.Disposables);


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

            //接続の設定
            this.ConnectionConfigCommand = new ReactiveCommand()
                .WithSubscribe(x =>
                {
                    new SettingWindow()
                    {
                        Owner = this.View,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    }
                    .ShowDialog();
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
            player.Message.Subscribe(y => this.WriteNotice(y.Text, false,
                (y.Type == StatusType.Message) ? LogTypes.MacroMessage : LogTypes.Notice))
                .AddTo(this.Disposables);

            //マクロエラー
            player.Error.Subscribe(y => this.WriteNotice(y, false, LogTypes.Error))
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

                    if (!prev && y)
                    {
                        this.ScrollToBottom();
                    }
                })
                .AddTo(this.Disposables);

            this.ActionQueueSubject = new Subject<RequestContainer>().AddTo(this.Disposables);



            this.ActionQueueSubject
                .Buffer(TimeSpan.FromMilliseconds(20))
                .Where(x => x.Count > 0)
                .Subscribe(x => this.WriteMain(x))
                .AddTo(this.Disposables);

            //this.ActionQueueSubject
            //    .BufferVariable
            //        (TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(1000),
            //        100, Scheduler.Default)
            //    .Where(_ => !this.IsDisposed)
            //    .Subscribe(x => this.WriteMain(x))
            //    .AddTo(this.Disposables);


            this.PortName.Skip(1).Subscribe(name =>
            {
                if (!this.IsPortOpen.Value && name != null && name.Length > 0)
                {
                    this.OpenPortCommand.Execute();
                }
            })
            .AddTo(this.Disposables);

        }

        private void WriteMain(IEnumerable<RequestContainer> items)
        {
            var length = 0;
            var texts = new List<string>();
            var brushs = new List<TextBrush>();

            var scroll = false;

            var isLastNewLine = this.AppendableTextController.LastText?.Length <= 0;

            foreach (var item in items)
            {
                var text = item.Text;
                var newLine = (item.NewLine && !isLastNewLine);

                if (newLine)
                {
                    text = "\n" + text;
                }

                if (text.Length > 0)
                {
                    texts.Add(text);

                    if (item.Brush != null)
                    {
                        brushs.Add(item.Brush.AddOffset((newLine) ? length + 1 : length));
                    }

                    length += text.Length;

                    var lastChar = text[text.Length - 1];
                    isLastNewLine = lastChar == '\n' || lastChar == '\r';
                }

                if (item.Scroll)
                {
                    scroll = true;
                }
            }

            this.AppendableTextController.Write(string.Concat(texts), brushs.ToArray());

            if (scroll)
            {
                this.ScrollToBottom();
            }
        }

        private class RequestContainer
        {
            public string Text { get; set; }
            public TextBrush Brush { get; set; }
            public bool Scroll { get; set; }
            public bool NewLine { get; set; }
        }



        /// <summary>
        /// 末尾までスクロール
        /// </summary>
        private void ScrollToBottom() => this.AppendableTextController.ScrollToBottom();


        /// <summary>
        /// コンソールに文字列を表示
        /// </summary>
        /// <param name="text"></param>
        private void Write(string text, bool feed, bool isBold = false)
        {
            if (!this.IsLogFollowing.Value)
            {
                return;
            }

            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            var brush = (isBold) ? new TextBrush(true, 0, text.Length) : null;

            if (feed)
            {
                text += "\n";
            }

            this.ActionQueueSubject.OnNext(new RequestContainer()
            {
                Text = text,
                Brush = brush,
                Scroll = false,
                NewLine = false
            });
        }

        /// <summary>
        /// 通知文字列を表示
        /// </summary>
        /// <param name="text"></param>
        /// <param name="forceScroll"></param>
        /// <param name="type"></param>
        private void WriteNotice(string text, bool forceScroll, LogTypes type)
        {
            if (!this.IsLogFollowing.Value)
            {
                return;
            }
            if (type == LogTypes.Notice)
            {
                Application.Current.Dispatcher.Invoke
                    (() => { if (!this.IsDisposed) { this.NoticeText.Value = text; } });

                if (!this.IsNoticeEnabled.Value)
                {
                    return;
                }
            }
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            this.ActionQueueSubject.OnNext(new RequestContainer()
            {
                Text = text + "\n",
                Brush = new TextBrush(type.GetColor(), 0, text.Length),
                Scroll = forceScroll,
                NewLine = true,
            });

            //this.ActionQueueSubject.OnNext(() =>
            //{
            //    if (this.AppendableTextController.LastText?.Length > 0)
            //    {
            //        text = "\n" + text;
            //    }
            //
            //    this.WriteMain(text + "\n", new TextBrush(type.GetColor(), 0, text.Length));
            //
            //    if (forceScroll)
            //    {
            //        this.ScrollToBottom();
            //    }
            //});
        }


        /// <summary>
        /// TextBoxの内容が変化したときの処理
        /// </summary>
        /// <param name="textBox"></param>
        public void SubscribeTextChanged(TextBox textBox)
        {

            var textChanged =
                Observable.FromEvent<TextChangedEventHandler, TextChangedEventArgs>
                (h => (sender, e) => h(e),
                h => textBox.TextChanged += h,
                h => textBox.TextChanged -= h);

            //カーソルを末尾に移動させる
            this.TextHistoryIndex
                .Buffer(textChanged.Where(y => textBox.Text.Length > 0))
                .Where(y => y.Count > 0)
                .Subscribe(y => textBox.Select(textBox.Text.Length, 0))
                .AddTo(this.Disposables);

            textChanged
                .Where(x => this.TextHistory.ContainsIndex(this.TextHistoryIndex.Value)
                    && textBox.Text != this.TextHistory[this.TextHistoryIndex.Value])
                .Subscribe(x =>
                {
                    this.InputText = textBox.Text;
                    this.textEdited = true;
                    //this.TextHistoryIndex.Value = this.TextHistory.Count;
                })
                .AddTo(this.Disposables);

        }


        /// <summary>
        /// 入力履歴を進める
        /// </summary>
        private void IncrementHistoryIndex()
        {
            var currentIndex = (this.textEdited) ? this.TextHistory.Count : this.TextHistoryIndex.Value;

            if (this.InputText.Length > 0)
            {

                var index = currentIndex + 1;

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
            else if (currentIndex < this.TextHistory.Count)
            {
                this.TextHistoryIndex.Value = currentIndex + 1;
            }
        }

        /// <summary>
        /// 入力履歴をさかのぼる
        /// </summary>
        private void DecrementHistoryIndex()
        {
            var currentIndex = (this.textEdited) ? this.TextHistory.Count : this.TextHistoryIndex.Value;

            if (currentIndex == this.TextHistory.Count)
            {
                this.InputText = this.RequestedText.Value;
            }

            if (this.InputText.Length > 0)
            {

                var index = currentIndex - 1;

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
            else if (currentIndex > 0)
            {
                this.TextHistoryIndex.Value = currentIndex - 1;
            }
        }

        /// <summary>
        /// ファイルを選択してマクロを実行
        /// </summary>
        private void StartMacro()
        {

            var dialog = new OpenFileDialog();
            dialog.Filter = "C# code(*.cs)|*.cs|All Files(*.*)|*.*";
            if (dialog.ShowDialog() != true)
            {
                return;
            }


            var path = dialog.FileName;

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
            var sc = new ScriptMacro(name, code);

            this.ScrollToBottom();

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
                try
                {
                    var index = listDialog.Index;

                    var plugin = plugins.FromIndexOrDefault(index);
                    if (plugin != null)
                    {
                        this.Core.LaunchPluginUI(plugin);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }
    }
}

