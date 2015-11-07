using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Boredbone.XamlTools.ViewModel;
using Reactive.Bindings.Extensions;
using Terminal.Macro.Api;
using Terminal.Models.Macro;
using Terminal.Models.Serial;
using Terminal.Views;

namespace Terminal.Models
{
    public class ApplicationCore : ViewModelBase
    {
        //private static ApplicationCore _instance = new ApplicationCore();
        //public static ApplicationCore Instance => _instance;

        public ConnectionBase Connection { get; }
        public MacroPlayer MacroPlayer { get; }

        private PluginLoader<IActivator> PluginLoader { get; }

        public IActivator[] Plugins { get; }


        public ApplicationCore()
        {
#if SIMULATION
            this.Connection = new ConnectionSimulator().AddTo(this.Disposables);
#else
            this.Connection = new SerialConnection().AddTo(this.Disposables);
#endif
            this.Connection.LineCode = LineCodes.Lf;

            this.MacroPlayer = new MacroPlayer(this.Connection).AddTo(this.Disposables);

            this.PluginLoader = new PluginLoader<IActivator>("plugins").AddTo(this.Disposables);
            this.Plugins = this.PluginLoader.Plugins.ToArray();

            foreach(var activator in this.Plugins)
            {
                activator.AddTo(this.Disposables);

                //var activator = new FrequencyResponseAnalyzer.Activator();
                var module = activator.Activate(this.MacroPlayer);

                this.MacroPlayer.Modules.Register(module);

                //activator.OpenWindowRequested += o =>
                //{
                //    var window = new ModuleWindow();
                //    window.Content = o;
                //    window.Show();
                //    window.Activate();
                //};
                //activator.LaunchUI();
            }

            //TODO module
            this.MacroPlayer.Modules.Register(new ModuleSample(this.MacroPlayer));
            //this.MacroPlayer.RegisterModule("Module2", null);
            //this.MacroPlayer.RegisterModule("Module3", null);
        }
    }
}
