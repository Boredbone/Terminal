using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Macro.Api;

namespace Terminal.Models.Macro
{
    /// <summary>
    /// ScriptMacro内からアクセスできるグローバルクラス
    /// </summary>
    public class MacroGlobal
    {
        public IMacroEngine Macro { get; }
        public IPluginManager Plugins { get; }

        public MacroGlobal(IMacroEngine macro, IPluginManager plugins)
        {
            this.Macro = macro;
            this.Plugins = plugins;
        }
    }
}
