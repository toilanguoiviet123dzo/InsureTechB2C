using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Models
{
    [Collection("ServiceList")]
    public class mdServiceList : Entity
    {        
        public string ServiceName { get; set; } = "";
        public string Descriptions { get; set; } = "";
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public string Url { get; set; } = "";        
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }      
        
    }
}
