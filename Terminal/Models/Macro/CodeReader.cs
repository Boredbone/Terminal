using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terminal.Macro.Api;

namespace Terminal.Models.Macro
{

    /// <summary>
    /// 文字列をC#コードとして解釈し，MacroAttributeの付いたメソッドを取りだす
    /// </summary>
    public class CodeReader
    {
        private const string attributeName = nameof(MacroAttribute);

        /// <summary>
        /// MacroAttributeの付いたメソッドを取りだす
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public CodeBlock[] GetBlocks(string text)
        {
            return CSharpSyntaxTree
                .ParseText(text)//文字列から構文木を生成
                .GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()//全メソッドを列挙
                .Select(method => new
                {
                    Method = method,
                    Attribute = method.AttributeLists
                    .SelectMany(y => y.Attributes)
                    .FirstOrDefault(y =>
                    {
                        //Macro属性を抽出
                        var name = y.Name.ToString();
                        return name.Equals(attributeName) || (name + "Attribute").Equals(attributeName);
                    })
                })
                .Where(y => y.Attribute != null && y.Method.Body != null)
                .Select(y =>
                {
                    //Macro属性に引数が設定されている場合はそれを名前とする
                    //引数が無い場合はメソッド名を名前とする
                    var name = y.Attribute.ArgumentList?.Arguments.FirstOrDefault()?.ToString().Trim('"')
                        ?? y.Method.Identifier.ToString();
                    return new CodeBlock(name) { Code = y.Method.Body.ToString() };
                })
                .ToArray();
        }
    }
}
