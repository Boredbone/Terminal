using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
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
        /// <param name="Modules"></param>
        /// <returns></returns>
        public async Task RunAsync(IMacroEngine Macro, ModuleManager Modules)
        {

            //if (!initialized)
            //{
            //    await CSharpScript.Create("").RunAsync();
            //    initialized = true;
            //}


            var global = new MacroGlobal(Macro, Modules);

            var spaces = new[] {
                "System",
                "System.Collections",
                "System.Collections.Generic",
                "System.Threading.Tasks",
                "System.Text",
                "System.IO",
                "System.Linq",
                typeof(IModule).Namespace
            }
            .Union(Modules.NameSpaces);

            var assemblies = new[] {
                typeof(object).Assembly,
                typeof(Enumerable).Assembly,
                typeof(IModule).Assembly
            }
            .Union(Modules.Assemblies);

            var options = ScriptOptions.Default
                .WithNamespaces(spaces)
                .WithReferences(assemblies);

            var script = CSharpScript.Create(this.Code, options, typeof(MacroGlobal));

            //Macro.Start(this.Name);
            try
            {
                var state = await script.RunAsync(global);

                //送信バッファを空にする
                await Macro.SendAsync(null);
            }
            finally
            {
                //Macro.End(this.Name);
            }
        }
    }
}
