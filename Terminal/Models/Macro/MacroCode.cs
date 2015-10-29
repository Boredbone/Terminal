using System;
using System.Threading.Tasks;

namespace Terminal.Models.Macro
{
    public interface IMacroCode
    {
        Task RunAsync(MacroEngine Macro);
    }
}
