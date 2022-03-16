using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cores.Configs
{
    public class GrpcConfig
    {
        public int MaxChannelCount { get; set; }
        public string SystemConfigUrl { get; set; }
    }
}
