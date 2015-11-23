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
        public Type[] ActivePluginWindowTypes { get; }

        public LaunchUIEventArgs(Type[] activePluginWindowTypes)
        {
            this.ActivePluginWindowTypes = activePluginWindowTypes;
        }
    }
}
