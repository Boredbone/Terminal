using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Macro.Api
{
    public interface IActivator : IDisposable
    {
        string Name { get; }
        IModule Activate(IMacroPlayer player);
        bool LaunchUI();
        Action<object> OpenWindowRequested { get; set; }
    }
}
