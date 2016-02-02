using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Macro.Api
{
    /// <summary>
    /// プラグインからのウインドウ表示要求引数
    /// </summary>
    public class OpenWindowRequestArgs
    {
        /// <summary>
        /// ウインドウ内に表示するコントロール
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// ウインドウタイトル
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 幅
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 高さ
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// ウインドウに固有のID
        /// </summary>
        public string WindowId { get; set; }

        /// <summary>
        /// ウインドウを表示しない
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// ウインドウ高さをコンテンツに合わせる
        /// </summary>
        public bool SizeToHeight { get; set; }

        /// <summary>
        /// ウインドウ幅をコンテンツに合わせる
        /// </summary>
        public bool SizeToWidth { get; set; }

    }
}
