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
using Boredbone.Utility;
using System.Reactive.Linq;
using System.Resources;

namespace Terminal.Models
{
    /// <summary>
    /// アプリケーションのメインモデル
    /// </summary>
    public class ApplicationCore : ViewModelBase
    {


        private const string DefaultSaveDirectoryName = @"\Boredbone\Terminal";
        private const string settingsFileName = "appsettings.config";
        private const string placementFileName = "placement.config";

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

        private ApplicationSettings Settings { get; set; }
        private XmlSettingManager<ApplicationSettings> SettingsXml { get; }
        
        public string PortName
        {
            get { return this.Settings.PortName; }
            set
            {
                if (this.Settings.PortName != value)
                {
                    this.Settings.PortName = value;
                    RaisePropertyChanged(nameof(PortName));
                }
            }
        }



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

            var directoryName = DefaultSaveDirectoryName;

            var str = this.GetResourceString("SaveDirectoryName");
            if (str != null)
            {
                directoryName = str;
            }

            var saveDirectory = System.Environment.GetFolderPath
                (Environment.SpecialFolder.LocalApplicationData) + directoryName;

            if (!System.IO.Directory.Exists(saveDirectory))
            {
                System.IO.Directory.CreateDirectory(saveDirectory);
            }

            //ストレージに保存する設定
            this.SettingsXml = new XmlSettingManager<ApplicationSettings>(settingsFileName);

            this.SettingsXml.Directory = saveDirectory;

            this.Settings = SettingsXml
                .LoadXml(XmlLoadingOptions.IgnoreAllException)
                .Value;
            
            //ポートオープン時に名前を保存
            this.Connection.IsOpenChanged
                .Where(y => y)
                .Subscribe(y =>
                {
                    this.PortName = this.Connection.PortName;
                })
                .AddTo(this.Disposables);

            //マクロ
            this.MacroPlayer = new MacroPlayer(this.Connection).AddTo(this.Disposables);

            //ウィンドウ配置
            this.WindowPlacement = new RestoreWindowPlace.RestoreWindowPlace(placementFileName);

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

            //Plugin
            this.MacroPlayer.Plugins.Register(new PluginSample(this.MacroPlayer));

            //ポート一覧を更新
            this.Connection.RefreshPortNames();

            //ポートオープン
            var port = this.Connection.PortNames.FirstOrDefault(y => y.Equals(this.PortName));
            if (port != null)
            {
                this.Connection.Open(port);
            }
            else
            {
                var name = this.Connection.PortNames.FirstOrDefault();

                if (this.Connection.PortNames.Count == 1 && name != null)
                {
                    this.Connection.Open(name);
                }
            }



            Task.Run(async () => await ScriptMacro.InitializeAsync());
        }

        /// <summary>
        /// 設定ファイルをローカルストレージに保存
        /// </summary>
        public void Save()
        {
            try
            {
                this.WindowPlacement.Save();
                this.SettingsXml.SaveXml(this.Settings);
            }
            catch
            {

            }

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
            var args = new LaunchUiArgs(this.GetActivePluginWindowTypes());
            return plugin.LaunchUI(args);
        }


        private string GetResourceString(string key)
        {
            try
            {
                using (var resxSet = new ResXResourceSet(@"Resources\SpecificResource.resx"))
                {
                    return resxSet.GetString(key);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
