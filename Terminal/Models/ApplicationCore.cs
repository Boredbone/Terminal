using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Boredbone.XamlTools.ViewModel;
using Reactive.Bindings.Extensions;
using Terminal.Macro.Api;
using Terminal.Models.Macro;
using Terminal.Models.Serial;
using Terminal.Views;
using Boredbone.Utility.Extensions;

namespace Terminal.Models
{
    /// <summary>
    /// アプリケーションのメインモデル
    /// </summary>
    public class ApplicationCore : ViewModelBase
    {
        //private static ApplicationCore _instance = new ApplicationCore();
        //public static ApplicationCore Instance => _instance;

        /// <summary>
        /// 通信
        /// </summary>
        public ConnectionBase Connection { get; }

        /// <summary>
        /// マクロ管理
        /// </summary>
        public MacroPlayer MacroPlayer { get; }

        /// <summary>
        /// プラグイン
        /// </summary>
        public IActivator[] Plugins { get; }
        private PluginLoader<IActivator> PluginLoader { get; }

        /// <summary>
        /// ウィンドウ配置
        /// </summary>
        public RestoreWindowPlace.RestoreWindowPlace WindowPlacement { get; }


        public ApplicationCore()
        {
            //通信を初期化
#if SIMULATION
            //通信シミュレーション
            this.Connection = new ConnectionSimulator().AddTo(this.Disposables);
#else
            //シリアル通信
            this.Connection = new SerialConnection().AddTo(this.Disposables);
#endif
            //通信の改行コードを設定
            this.Connection.LineCode = LineCodes.Lf;

            //マクロ
            this.MacroPlayer = new MacroPlayer(this.Connection).AddTo(this.Disposables);

            //ウィンドウ配置
            this.WindowPlacement = new RestoreWindowPlace.RestoreWindowPlace("placement.config");

            //配下のフォルダからIActivatorを実装したプラグインのdllを読み込み
            this.PluginLoader = new PluginLoader<IActivator>("plugins").AddTo(this.Disposables);
            this.Plugins = this.PluginLoader.Plugins.ToArray();

            //各プラグインを初期化
            foreach(var activator in this.Plugins)
            {
                activator.AddTo(this.Disposables);
                
                //モジュールを読み込み
                var plugin = activator.Activate(this.MacroPlayer);

                //マクロから使えるように登録
                this.MacroPlayer.Plugins.Register(plugin);
                
                //UI起動要求時の処理
                activator.OpenWindowRequested = args =>
                {
                    //ウィンドウ生成
                    var window = new PluginWindow()
                    {
                        Content = args.Content,
                        Title = args.Title ?? activator.Name
                    };

                    //ウィンドウサイズが指定されている場合は反映
                    if (args.Width >= 64)
                    {
                        window.Width = args.Width;
                    }
                    if (args.Height >= 64)
                    {
                        window.Height = args.Height;
                    }

                    //前回終了時の配置が保存されている場合は復元
                    if (args.WindowId != null && args.WindowId.Length > 0)
                    {
                        this.WindowPlacement.Register(window, args.WindowId);
                    }

                    //window.Owner = this;

                    //表示
                    window.Show();
                    window.Activate();

                    return window;
                };
            }

            //TODO plugin
            this.MacroPlayer.Plugins.Register(new PluginSample(this.MacroPlayer));

            Task.Run(async () => await ScriptMacro.InitializeAsync());
        }

        /// <summary>
        /// 設定ファイルをローカルストレージに保存
        /// </summary>
        public void Save()
        {
            this.WindowPlacement.Save();
        }


        private Type[] GetActivePluginWindowTypes()
        {
            return Application.Current
               .Windows
               .AsEnumerableWithSafeCast<PluginWindow>()
               .Select(y => y.Content.GetType())
               .ToArray();
        }

        public bool LaunchPluginUI(IActivator plugin)
        {
            var args = new LaunchUIEventArgs(this.GetActivePluginWindowTypes());
            return plugin.LaunchUI(args);
        }
    }
}
