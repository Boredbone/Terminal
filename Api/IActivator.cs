using System;

namespace Terminal.Macro.Api
{
    /// <summary>
    /// 
    /// </summary>
    public interface IActivator : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        IPlugin Activate(IMacroPlayer player);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool LaunchUI(LaunchUIEventArgs args);

        /// <summary>
        /// 
        /// </summary>
        Func<OpenWindowRequestEventArgs, object> OpenWindowRequested { get; set; }
    }
}
