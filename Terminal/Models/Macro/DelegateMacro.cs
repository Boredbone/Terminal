using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Models.Serial;

namespace Terminal.Models.Macro
{
    public class DelegateMacro : IMacroCode
    {
        private string Name { get; }
        private Func<IMacroEngine, IReadOnlyDictionary<string, IModule>, Task> AsyncFunc { get; }

        public DelegateMacro(string name,
            Func<IMacroEngine, IReadOnlyDictionary<string, IModule>, Task> asyncFunc)
        {
            this.Name = name;
            this.AsyncFunc = asyncFunc;
#if SIMULATION
            if (asyncFunc == null)
            {
                this.AsyncFunc = async (Macro, Modules) =>
                {


                    //var module = Modules["Module0"];
                    //
                    //module["parameter1"] = 1;
                    //module["parameter2"] = "text";
                    //
                    //var currentParameter = module["parameter3"];
                    //
                    //var result = await module.RunAsync(null);

                    Macro.Timeout = 0;

                    await Macro.SendAsync("text1");

                    await Macro.WaitAsync("\n>");


                    await Macro.DelayAsync(1000);



                    await Macro.SendAsync("text2");

                    await Macro.WaitLineAsync();


                    await Macro.SendAsync("text3");

                    //Macro.Pause();
                    //await Task.Delay(2000);
                    //Macro.Resume();

                    await Macro.WaitAsync("\n>");

                    //Macro.Cancel();


                    var fileName = @"d.txt";

                    var line = "";
                    var list = new List<string>();

                    using (var sr = new StreamReader(fileName, Encoding.UTF8))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            list.Add(line);
                        }
                    }

                    foreach (var str in list)
                    {
                        Macro.Display(str);
                    }



                    Macro.Timeout = 4000;

                    await Macro.SendAsync("text4");

                    try
                    {
                        await Macro.WaitAsync("\ny>");
                    }
                    catch (TimeoutException)
                    {
                        Macro.Display("timeout");
                        await Macro.SendAsync("retry");
                    }


                    await Macro.DelayAsync(3000);

                    await Macro.SendAsync("text5");

                    await Macro.WaitAsync("\ny>");
                };
            }
#endif
        }

        public async Task RunAsync(MacroEngine Macro, IReadOnlyDictionary<string, IModule> Modules)
        {
            Macro.Start(this.Name);
            try
            {
                await this.AsyncFunc(Macro, Modules);

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
