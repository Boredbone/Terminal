using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Models.Macro
{
    public interface IModule
    {
        //object this[string key] { get; set; }
        //void SetParameter(string key, object value);
        //Task<object> RunAsync(object arg);
        IMacroEngine Engine { get; set; }
    }
}
