using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Macro.Api
{
    public class OpenWindowRequestEventArgs
    {
        public object Content { get; set; }
        public string Title { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string WindowId { get; set; }
    }
}
