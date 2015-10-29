using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Boredbone.Utility.Extensions;

namespace Boredbone.Utility
{
#if !WINDOWS_APP
    public class TextLoader
    {
        /// <summary>
        /// 指定パスのファイルを開き，文字列として読み込む
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public string Load(string filepath)
        {
            string text = "";

            //テキストファイルを開く
            using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                byte[] bs = new byte[fs.Length];
                //byte配列に読み込む
                fs.Read(bs, 0, bs.Length);

                //文字コードを取得する
                var enc = bs.GetCode();


                //UTF-8のBOM除去
                if (enc == Encoding.UTF8 && bs.Length > 3
                    && bs[0] == 0xEF && bs[1] == 0xBB && bs[2] == 0xBF)
                {
                    text = enc.GetString(bs.Skip(3).ToArray());
                }
                else
                {
                    text = enc.GetString(bs);
                }
            }
            return text;
        }
    }
#endif
}
