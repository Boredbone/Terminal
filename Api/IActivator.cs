﻿using System;

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
        IModule Activate(IMacroPlayer player);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool LaunchUI(LaunchUIEventArgs args);

        /// <summary>
        /// 
        /// </summary>
        Action<OpenWindowRequestEventArgs> OpenWindowRequested { get; set; }
    }
}
