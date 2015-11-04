
namespace Terminal.Macro.Api
{
    /// <summary>
    /// マクロから利用可能な外部モジュール
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// モジュールに設定されたMacroEngine
        /// </summary>
        IMacroEngine Engine { get; set; }
    }
}
