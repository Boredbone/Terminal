using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Terminal.Macro.Api
{
    public interface IMacroCode
    {
        string Name { get; }
        Task RunAsync(IMacroEngine Macro, ModuleManager Modules);
    }
}
