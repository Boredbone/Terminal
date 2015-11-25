using System;
using System.Threading.Tasks;

namespace Terminal.Macro.Api
{
    /// <summary>
    /// マクロの実行環境
    /// </summary>
    public interface IMacroPlayer
    {
        /// <summary>
        /// 現在実行中のマクロ
        /// </summary>
        IMacroEngine Engine { get; }

        /// <summary>
        /// 登録されているプラグイン
        /// </summary>
        IPluginManager Plugins { get; }

        /// <summary>
        /// 実行状態の変化イベント
        /// </summary>
        IObservable<bool> IsExecutingChanged { get; }

        /// <summary>
        /// 一時停止・再開イベント
        /// </summary>
        IObservable<bool> IsPausing { get; }

        /// <summary>
        /// マクロの実行を中断
        /// </summary>
        void Cancel();

        /// <summary>
        /// マクロを一時停止・再開
        /// </summary>
        void PauseOrResume();

        /// <summary>
        /// マクロの実行を開始
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="asyncFunc">実行する関数のデリゲート</param>
        void Start(string name, Func<IMacroEngine, IPluginManager, Task> asyncFunc);
    }
}