namespace Terminal.Macro.Api
{
    /// <summary>
    /// 登録されたモジュールを管理
    /// </summary>
    public interface IModuleManager
    {
        /// <summary>
        /// 指定した型のモジュールを取得
        /// </summary>
        /// <typeparam name="T">モジュールの型</typeparam>
        /// <returns>モジュールのインスタンス</returns>
        T Get<T>() where T : IModule;
    }
}