using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Models.Macro
{
    /// <summary>
    /// マクロとして使用できることを示す属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class MacroAttribute : Attribute
    {
        public string Name { get; }

        /// <summary>
        /// 名前付きのマクロを指定
        /// </summary>
        /// <param name="name"></param>
        public MacroAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// マクロを指定
        /// </summary>
        public MacroAttribute()
        {
            this.Name = null;
        }
    }
}
