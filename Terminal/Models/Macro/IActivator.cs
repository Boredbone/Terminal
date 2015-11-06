using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Macro.Api;

namespace Terminal.Models.Macro
{
    public interface IActivator : IDisposable
    {
        IModule Activate(MacroPlayer player);
        bool LaunchUI();
        event Action<object> OpenWindowRequested;
    }
}
