using System;

namespace Terminal.Macro.Api
{
    /// <summary>
    /// プラグインを起動させるためのオブジェクト
    /// </summary>
    public interface IActivator : IDisposable
    {
        /// <summary>
        /// プラグインの名前
        /// </summary>
        string Name { get; }

        /// <summary>
        /// プラグインの起動
        /// </summary>
        /// <param name="player">マクロの実行環境</param>
        /// <returns>プラグインのインスタンス</returns>
        IPlugin Activate(IMacroPlayer player);

        /// <summary>
        /// プラグインのUIを表示
        /// </summary>
        /// <param name="args">イベント引数</param>
        /// <returns>表示の成否</returns>
        bool LaunchUI(LaunchUiArgs args);

        /// <summary>
        /// プラグインからウインドウを表示するためのデリゲート
        /// </summary>
        Func<OpenWindowRequestArgs, object> OpenWindowRequested { get; set; }
    }
}
