using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Client.Common
{
    public class BrowserDimension
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
    public class CodeNameModel
    {
        public string CodeString { get; set; } = "";
        public int CodeInt { get; set; }
        public string Name { get; set; } = "";
    }
}
