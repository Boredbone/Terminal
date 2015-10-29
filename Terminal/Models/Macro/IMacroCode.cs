using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Terminal.Models.Macro
{
    public interface IMacroCode
    {
        Task RunAsync(MacroEngine Macro, IReadOnlyDictionary<string, IModule> Modules);
    }
}
