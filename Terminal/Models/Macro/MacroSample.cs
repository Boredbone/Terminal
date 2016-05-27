using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Linq;
using Terminal.Macro.Api;

namespace Terminal.Models.Macro
{
    class MacroSample
    {
        [Macro]
        public async Task Sample(IMacroEngine Macro, IPluginManager Plugins)
        {

            Macro.Timeout = 0;

            await Macro.SendLineAsync("cs1");

            await Macro.WaitAsync("\n>");


            //var num = 2;
            await Macro.DelayAsync(1000);



            await Macro.SendLineAsync("cs2");

            await Macro.WaitLineAsync();


            await Macro.SendLineAsync("cs3");

            //Macro.Pause();
            //await Task.Delay(2000);
            //Macro.Resume();

            await Macro.WaitAsync("\n>");

            Macro.Display(Macro.History(0));
            Macro.Display(Macro.History(1));
            Macro.Display(Macro.History(2));
            Macro.Display(Macro.History(3));

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
                //await Task.Delay(10);
            }
            //await Task.Delay(500);

            Macro.Timeout = 4000;

            await Macro.SendLineAsync("cs4");

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

            await Macro.SendLineAsync("cs5");

            await Macro.WaitAsync("\ny>");
            
        }

        //[Macro("Plugin")]
        //public async Task PluginSample(IMacroEngine Macro, IPluginManager Plugins)
        //{
        //
        //
        //    var pluin = Plugins.Get<PluginSample>();
        //
        //    Macro.Display(pluin.ToString());
        //
        //    pluin.Parameter1 = 1;
        //    pluin.Parameter2 = "text";
        //    
        //    var currentParameter = pluin.Parameter3;
        //    
        //
        //    var result = await pluin.RunAsync();
        //    
        //}

        [Macro]
        public async Task Execute(IMacroEngine Macro, IPluginManager Plugins)
        {
            await Macro.DelayAsync(10);
        }
    }
}
