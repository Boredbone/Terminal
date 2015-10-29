using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Models.Macro
{
    public class CodeBlock
    {
        public string Name { get; }
        public List<string> Codes { get; }
        public CodeBlock(string name)
        {
            this.Name = name;
            this.Codes = new List<string>();
        }
        public void Add(string text) => this.Codes.Add(text);
    }
}
