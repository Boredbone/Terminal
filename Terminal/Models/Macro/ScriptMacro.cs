using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Boredbone.Utility.Extensions;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;

namespace Terminal.Models.Macro
{

    public class ScriptMacro : IMacroCode
    {
        private string Code { get; }
        private string Name { get; }

        public ScriptMacro(string name, string code)
        {
            this.Name = name;
            this.Code = code;
        }

        //public void Load(string filePath)
        //{
        //    string text = "";
        //
        //    //テキストファイルを開く
        //    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        //    {
        //        byte[] bs = new byte[fs.Length];
        //        //byte配列に読み込む
        //        fs.Read(bs, 0, bs.Length);
        //
        //        //文字コードを取得する
        //        var enc = bs.GetCode();
        //
        //        text = enc.GetString(bs);
        //    }
        //
        //    this.Code = text;
        //    this.Name = Path.GetFileNameWithoutExtension(filePath);
        //}

        public async Task RunAsync(MacroEngine Macro, IReadOnlyDictionary<string, IModule> Modules)
        {
            var global = new MacroGlobal(Macro, Modules);

            var options = ScriptOptions.Default
                .WithNamespaces(
                "System",
                "System.Collections",
                "System.Collections.Generic",
                "System.Threading.Tasks",
                "System.Text",
                "System.IO",
                "System.Linq")
                .WithReferences(
                typeof(object).Assembly,
                typeof(Enumerable).Assembly);

            var script = CSharpScript.Create(this.Code, options, typeof(MacroGlobal));

            Macro.Start(this.Name);
            try
            {
                var state = await script.RunAsync(global);

                //ユーザー指定コードの実行後に必ず行う
                await Macro.SendAsync(null);
            }
            finally
            {
                Macro.End(this.Name);
            }
        }
    }
}
