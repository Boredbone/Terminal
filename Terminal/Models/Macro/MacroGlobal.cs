﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Models.Macro
{
    /// <summary>
    /// ScriptMacro内からアクセスできるグローバルクラス
    /// </summary>
    public class MacroGlobal
    {
        public IMacroEngine Macro { get; }
        public IModuleManager Modules { get; }

        public MacroGlobal(IMacroEngine macro, IModuleManager modules)
        {
            this.Macro = macro;
            this.Modules = modules;
        }
    }
}
