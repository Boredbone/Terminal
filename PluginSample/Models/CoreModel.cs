using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Terminal.Macro.Api;

namespace PluginSample.Models
{
    public class CoreModel
    {
        public Plugin Plugin { get; }
        private IMacroPlayer Player { get; }
        public event Func<OpenWindowRequestArgs, object> OpenWindowRequested;


        public CoreModel(IMacroPlayer player)
        {
            this.Player = player;
            this.Plugin = new Plugin(player);
        }

        public object OpenWindow(OpenWindowRequestArgs args)
            => (Window)this.OpenWindowRequested?.Invoke(args);

    }
}
