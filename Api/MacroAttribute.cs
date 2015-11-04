using System;

namespace Terminal.Macro.Api
{
    /// <summary>
    /// マクロとして使用できることを示す属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class MacroAttribute : Attribute
    {
        /// <summary>
        /// 名前
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 名前付きのマクロを指定
        /// </summary>
        /// <param name="name">名前</param>
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
