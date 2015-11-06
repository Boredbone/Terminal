using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Macro.Api
{
    /// <summary>
    /// マクロ実行ステータスのコンテナ
    /// </summary>
    public class StatusItem
    {
        public string Text { get; }
        public StatusType Type { get; set; }

        public static int count = 0;

        public StatusItem(string text)
        {
            this.Text = text;
            this.Type = StatusType.Normal;
            count++;
        }
    }
}
