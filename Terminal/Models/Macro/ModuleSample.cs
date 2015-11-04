using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Macro.Api;

namespace Terminal.Models.Macro
{
    public class ModuleSample : IModule
    {
        public IMacroEngine Engine { get; set; }

        public int Parameter1 { get; set; }
        public string Parameter2 { get; set; }
        public int Parameter3 { get; set; }

        public async Task<string> RunAsync()
        {
            await this.Engine.SendAsync(this.Parameter2);
            await this.Engine.WaitAsync();
            return "";
        }
    }
}
