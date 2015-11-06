using System;
using System.Threading.Tasks;

namespace Terminal.Macro.Api
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMacroPlayer
    {
        /// <summary>
        /// 
        /// </summary>
        IMacroEngine Engine { get; }

        /// <summary>
        /// 
        /// </summary>
        IModuleManager Modules { get; }

        /// <summary>
        /// 
        /// </summary>
        IObservable<bool> IsExecuting { get; }

        /// <summary>
        /// 
        /// </summary>
        IObservable<bool> IsPausing { get; }

        /// <summary>
        /// 
        /// </summary>
        void Cancel();

        /// <summary>
        /// 
        /// </summary>
        void PauseOrResume();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="asyncFunc"></param>
        void Start(string name, Func<IMacroEngine, IModuleManager, Task> asyncFunc);
    }
}