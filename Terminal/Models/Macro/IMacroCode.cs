using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terminal.Macro.Api;

namespace Terminal.Models.Macro
{
    public interface IMacroCode
    {
        string Name { get; }
        Task RunAsync(IMacroEngine Macro, PluginManager Plugins);
    }
}
