using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Cores.Grpc.Client
{
    public class ServiceListModel
    {
        public string ID { get; set; } = "";
        [Required]
        public string ServiceName { get; set; } = "";
        public string Descriptions { get; set; } = "";
        public string Host { get; set; } = "";
        public int Port { get; set; }
        [Required]
        public string Url { get; set; } = "";                    
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }        
        public int UpdMode { get; set; }
    }    
}
