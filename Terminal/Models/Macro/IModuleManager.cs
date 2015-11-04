namespace Terminal.Models.Macro
{
    public interface IModuleManager
    {
        T Get<T>() where T : IModule;
    }
}