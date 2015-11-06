using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Macro.Api;

namespace Terminal.Macro.Api
{
    public class ModuleSample : IModule
    {
        public MacroPlayer Player { get; set; }

        public int Parameter1 { get; set; }
        public string Parameter2 { get; set; }
        public int Parameter3 { get; set; }

        public ModuleSample(MacroPlayer player)
        {
            this.Player = player;
        }

        public async Task<string> RunAsync()
        {
            await this.Player.Engine.SendAsync(this.Parameter2);
            await this.Player.Engine.WaitAsync();
            return "";
        }
    }
}
