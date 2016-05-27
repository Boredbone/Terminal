using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Macro.Api;

namespace Terminal.Models.Macro
{
    /// <summary>
    /// デリゲートをマクロとして実行
    /// </summary>
    public class DelegateMacro : IMacroCode
    {
        public string Name { get; }
        private Func<IMacroEngine, IPluginManager, Task> AsyncFunc { get; }

        public DelegateMacro(string name,
            Func<IMacroEngine, IPluginManager, Task> asyncFunc)
        {
            this.Name = name;
            this.AsyncFunc = asyncFunc;
#if SIMULATION
            if (asyncFunc == null)
            {
                this.AsyncFunc = async (Macro, Plugins) =>
                {


                    //Macro.Display(Plugins.ToString());
                    //
                    //var plugin = Plugins.Get<PluginSample>();
                    //
                    //plugin.Parameter1 = 1;
                    //plugin.Parameter2 = "text";
                    //
                    //var currentParameter = plugin.Parameter3;
                    //
                    //
                    //var result = await plugin.RunAsync();
                    //return;
                    //var plugin = Plugins["Plugin0"];
                    //
                    //plugin["parameter1"] = 1;
                    //plugin["parameter2"] = "text";
                    //
                    //var currentParameter = plugin["parameter3"];
                    //
                    //var result = await plugin.RunAsync(null);

                    Macro.Timeout = 0;

                    await Macro.SendLineAsync("text1");

                    await Macro.WaitAsync("\n>");


                    await Macro.DelayAsync(1000);



                    await Macro.SendLineAsync("text2");

                    await Macro.WaitLineAsync();


                    await Macro.SendLineAsync("text3");

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

                    await Macro.SendLineAsync("text4");

                    try
                    {
                        await Macro.WaitAsync("\ny>");
                    }
                    catch (TimeoutException)
                    {
                        Macro.Display("timeout");
                        await Macro.SendLineAsync("retry");
                    }


                    await Macro.DelayAsync(3000);

                    await Macro.SendLineAsync("text5");

                    await Macro.WaitAsync("\ny>");
                };
            }
#endif
        }

        /// <summary>
        /// マクロ実行
        /// </summary>
        /// <param name="Macro"></param>
        /// <param name="Plugins"></param>
        /// <returns></returns>
        public async Task RunAsync(IMacroEngine Macro, PluginManager Plugins)
        {
            //Macro.Start(this.Name);
            try
            {
                await this.AsyncFunc(Macro, Plugins);

                //送信バッファを空にする
                await Macro.SendLineAsync(null);
            }
            finally
            {
                //Macro.End(this.Name);
            }
        }
    }
}
