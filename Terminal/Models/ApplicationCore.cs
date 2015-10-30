using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Boredbone.XamlTools.ViewModel;
using Reactive.Bindings.Extensions;
using Terminal.Models.Macro;
using Terminal.Models.Serial;

namespace Terminal.Models
{
    public class ApplicationCore : ViewModelBase
    {
        //private static ApplicationCore _instance = new ApplicationCore();
        //public static ApplicationCore Instance => _instance;

        public IConnection Connection { get; }
        public MacroPlayer MacroPlayer { get; }


        public ApplicationCore()
        {
#if SIMULATION
            this.Connection = new ConnectionSimulator().AddTo(this.Disposables);
#else
            this.Connection = new SerialConnection().AddTo(this.Disposables);
#endif
            this.Connection.LineCode = LineCodes.Lf;

            this.MacroPlayer = new MacroPlayer(this.Connection).AddTo(this.Disposables);


            //TODO module
            this.MacroPlayer.Modules.Register(new ModuleSample());
            //this.MacroPlayer.RegisterModule("Module2", null);
            //this.MacroPlayer.RegisterModule("Module3", null);
        }
    }
}
