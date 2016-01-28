using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using Terminal.Macro.Api;
using System.ComponentModel.Composition;
using PluginSample.Views;
using PluginSample.ViewModels;
using PluginSample.Models;

namespace PluginSample
{
    [Export(typeof(IActivator))]
    public class Activator : IActivator
    {
        private string name = "Plugin Sample";
        public string Name => this.name;

        public Func<OpenWindowRequestArgs, object> OpenWindowRequested { get; set; }

        private CoreModel Core { get; set; }

        private CompositeDisposable Disposables { get; }


        public Activator()
        {
            this.Disposables = new CompositeDisposable();
        }


        public IPlugin Activate(IMacroPlayer player)
        {
            //起動済みの場合は例外
            if (this.Core != null)
            {
                throw new InvalidOperationException("Already activated");
            }
            //メインモデルを生成
            this.Core = new CoreModel(player);

            //プラグイン側からのウインドウ表示要求
            this.Core.OpenWindowRequested += args => this.OpenWindowRequested?.Invoke(args);

            //マクロ用のプラグインを返却
            return this.Core.Plugin;
        }

        public void Dispose()
        {
            this.Disposables.Dispose();
        }

        /// <summary>
        /// UIの表示
        /// </summary>
        /// <returns></returns>
        public bool LaunchUI(LaunchUiArgs args)
        {
            //実験管理ウィンドウ
            if (!args.ActivePluginWindowTypes.Contains(typeof(MainControl)))
            {
                var viewModel = new MainControlViewModel(this.Core);
                var control = new MainControl() { ViewModel = viewModel };
                this.OpenWindowRequested?.Invoke(new OpenWindowRequestArgs()
                {
                    Content = control,
                    Title = "Main Control",
                    WindowId = "MainControl",
                });
            }

            return true;
        }
    }
}
