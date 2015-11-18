using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Macro.Api
{
    /// <summary>
    /// 
    /// </summary>
    public class LaunchUIEventArgs
    {
        public Type[] ActiveModuleWindowTypes { get; }

        public LaunchUIEventArgs(Type[] activeModuleWindowTypes)
        {
            this.ActiveModuleWindowTypes = activeModuleWindowTypes;
        }
    }
}
