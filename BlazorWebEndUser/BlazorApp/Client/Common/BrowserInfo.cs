using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Client.Common
{
    public static class BrowserInfo
    {
        public static bool IsMobile { get; set; } = false;
        public static int Width { get; set; }
        public static int Height { get; set; }
    }
}
