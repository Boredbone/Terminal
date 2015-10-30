using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Linq;

namespace Terminal.Models.Macro
{
    class MacroSample
    {
        public async Task Sample(IMacroEngine Macro, ModuleManager Modules)
        {
            #region Sample

            Macro.Timeout = 0;

            await Macro.SendAsync("cs1");

            await Macro.WaitAsync("\n>");


            var num = 2;
            await Macro.DelayAsync(1000);



            await Macro.SendAsync("cs2");

            await Macro.WaitLineAsync();


            await Macro.SendAsync("cs3");

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
                //await Task.Delay(10);
            }
            //await Task.Delay(500);

            Macro.Timeout = 4000;

            await Macro.SendAsync("cs4");

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

            await Macro.SendAsync("cs5");

            await Macro.WaitAsync("\ny>");

            #endregion
        }

        public async Task ModuleSample(IMacroEngine Macro, ModuleManager Modules)
        {
            #region ModuleSample




            var module = Modules.Get<ModuleSample>();

            Macro.Display(module.ToString());

            module.Parameter1 = 1;
            module.Parameter2 = "text";
            
            var currentParameter = module.Parameter3;
            

            var result = await module.RunAsync();



            #endregion
        }

        public async Task Execute(IMacroEngine Macro, ModuleManager Modules)
        {
            #region Template

            #endregion
        }
    }
}
