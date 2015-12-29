using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Macro.Api
{
    /// <summary>
    /// プラグインへのUI表示要求の引数
    /// </summary>
    public class LaunchUiArgs
    {
        /// <summary>
        /// 現在表示中のウィンドウの型
        /// </summary>
        public Type[] ActivePluginWindowTypes { get; }

        /// <summary>
        /// プラグインへのUI表示要求の引数
        /// </summary>
        /// <param name="activePluginWindowTypes">現在表示中のウィンドウの型</param>
        public LaunchUiArgs(Type[] activePluginWindowTypes)
        {
            this.ActivePluginWindowTypes = activePluginWindowTypes;
        }
    }
}
