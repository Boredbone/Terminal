using System;
using System.Threading.Tasks;

namespace Terminal.Macro.Api
{
    public interface IMacroPlayer
    {
        IMacroEngine Engine { get; }
        IModuleManager Modules { get; }

        void Cancel();
        void PauseOrResume();
        //void Start(IMacroCode code);
        void Start(string name, Func<IMacroEngine, IModuleManager, Task> asyncFunc);
    }
}