using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Models.Macro
{
    /// <summary>
    /// 文字列をC#コードとして解釈し，#regionで囲まれたブロックを取りだす
    /// </summary>
    public class CodeReader
    {
        /// <summary>
        /// #regionで囲まれたブロックを取りだす
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IEnumerable<CodeBlock> GetBlocks(string text)
        {

            var t2 = new string(RemoveComment(text).ToArray());

            string line;
            var list = new List<string>();
            var sr = new StringReader(t2);

            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length > 0)
                {
                    list.Add(line);
                }
            }

            return this.DivideBlock(list);
        }

        /// <summary>
        /// コメント除去
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private IEnumerable<char> RemoveComment(string text)
        {
            var buf = text.ToCharArray();
            var len = buf.Length;
            var reteral = false;

            for (int i = 0; i < len; i++)
            {
                if (!reteral && buf[i] == '/' && (i + 1) < len && buf[i + 1] == '/')
                {
                    i += 2;
                    while (i < len && buf[i] != '\n') i++;
                }
                else if (!reteral && buf[i] == '/' && (i + 1) < len && buf[i + 1] == '*')
                {
                    i += 2;
                    while (i < len && !(buf[i] == '*' && (i + 1) < len && buf[i + 1] == '/')) i++;
                    i += 2;
                }
                else if ((buf[i] == '\"' || buf[i] == '\'') && (buf[i - 1] != '\\'))
                {
                    reteral = !reteral;
                }
                yield return buf[i];
            }
        }

        /// <summary>
        /// #region検出
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private IEnumerable<CodeBlock> DivideBlock(IEnumerable<string> str)
        {

            var regionNest = 0;
            CodeBlock codeBlock = null;
            //var inComment = false;

            //var splitter = new[] { "//" };

            foreach (var line in str)
            {

                var fixedLine = line.TrimStart();


                //var sp = line.Split(splitter, StringSplitOptions.None);
                //
                //if(!inComment && sp.Length > 0 && sp[0].Contains("/*"))
                //{
                //    inComment = true;
                //}

                ///**//*

                if (fixedLine.StartsWith("#region"))
                {
                    if (regionNest == 0)
                    {
                        codeBlock = new CodeBlock(fixedLine.Replace("#region", "").Trim());
                    }
                    regionNest++;
                }
                else if (fixedLine.StartsWith("#endregion"))
                {
                    if (regionNest <= 0)
                    {
                        throw new ArgumentException();
                    }
                    regionNest--;

                    if (regionNest == 0)
                    {
                        codeBlock.Add(line);
                        yield return codeBlock;
                        codeBlock = null;
                    }

                }

                if (codeBlock != null)
                {
                    codeBlock.Add(line);
                }
            }
        }

    }
}
