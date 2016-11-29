using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terminal.Macro.Api;

namespace Terminal.Models.Macro
{
    public class PluginManager : IPluginManager
    {

        private Dictionary<string, IPlugin> Dictionary { get; }
        public IMacroEngine Engine { get; set; }

        public HashSet<Assembly> Assemblies { get; }
        public HashSet<string> NameSpaces { get; }

        public PluginManager()
        {
            this.Dictionary = new Dictionary<string, IPlugin>();
            this.Assemblies = new HashSet<Assembly>();
            this.NameSpaces = new HashSet<string>();
        }

        public T Get<T>() where T : IPlugin
        {
            return (T)this.Dictionary[typeof(T).FullName];
        }

        public void Register(IPlugin value)
        {
            var type = value.GetType();

            this.Dictionary[type.FullName] = value;

            var assembly = type.Assembly;
            var space = type.Namespace;

            this.Assemblies.Add(assembly);
            this.NameSpaces.Add(space);

        }

    }
}
