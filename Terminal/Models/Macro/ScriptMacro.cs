using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Terminal.Macro.Api;

namespace Terminal.Models.Macro
{
    /// <summary>
    /// 文字列をC#コードとして実行
    /// </summary>
    public class ScriptMacro : IMacroCode
    {
        private string Code { get; }
        public string Name { get; }

        private static readonly string[] DefaultNamespaces
            = new[] {
                "System",
                "System.Collections",
                "System.Collections.Generic",
                "System.Threading.Tasks",
                "System.Text",
                "System.IO",
                "System.Linq",
                typeof(IPlugin).Namespace
            };

        private static readonly Assembly[] DefaultAssemblies
            = new[] {
                typeof(object).Assembly,
                typeof(Enumerable).Assembly,
                typeof(IPlugin).Assembly,
                typeof(System.ComponentModel.INotifyPropertyChanged).Assembly
            };


        //private static bool initialized = false;

        public ScriptMacro(string name, string code)
        {
            this.Name = name;
            this.Code = code;
        }

        /// <summary>
        /// マクロ実行
        /// </summary>
        /// <param name="Macro"></param>
        /// <param name="Plugins"></param>
        /// <returns></returns>
        public async Task RunAsync(IMacroEngine Macro, PluginManager Plugins)
        {

            //if (!initialized)
            //{
            //    await CSharpScript.Create("").RunAsync();
            //    initialized = true;
            //}


            var global = new MacroGlobal(Macro, Plugins);

            var spaces = DefaultNamespaces.Union(Plugins.NameSpaces);
            var assemblies = DefaultAssemblies.Union(Plugins.Assemblies);

            var options = ScriptOptions.Default
                .WithImports(spaces)
                .WithReferences(assemblies);

            //var script = CSharpScript.Create(this.Code, options, typeof(MacroGlobal));

            //Macro.Start(this.Name);
            try
            {
                await CSharpScript.RunAsync(this.Code, options, global, typeof(MacroGlobal));
                //var state = await script.RunAsync(global);

                //送信バッファを空にする
                await Macro.SendAsync(null);
            }
            finally
            {
                //Macro.End(this.Name);
            }
        }

        public static async Task InitializeAsync()
        {

            var spaces = DefaultNamespaces;
            var assemblies = DefaultAssemblies;

            var options = ScriptOptions.Default
                .WithImports(spaces)
                .WithReferences(assemblies);

            var code = @"
            var array = new[] { 1.0, 2.0, 3.0 };
            var result = array.Select(y => y * 2.0).ToArray().Length;";

            //var script = CSharpScript.Create(code, options);

            //try
            //{
            await CSharpScript.RunAsync(code, options);
            //var state = await script.RunAsync();
            //}
            //catch
            //{
            //}
        }
    }
}
