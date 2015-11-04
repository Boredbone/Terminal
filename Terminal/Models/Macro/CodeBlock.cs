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
        public string Code { get; set; }
        public CodeBlock(string name)
        {
            this.Name = name;
        }
    }
}
