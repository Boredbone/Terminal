
namespace Terminal.Macro.Api
{
    /// <summary>
    /// 登録されたプラグインを管理
    /// </summary>
    public interface IPluginManager
    {
        /// <summary>
        /// 指定した型のプラグインを取得
        /// </summary>
        /// <typeparam name="T">プラグインの型</typeparam>
        /// <returns>プラグインのインスタンス</returns>
        T Get<T>() where T : IPlugin;
        
    }
}