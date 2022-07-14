using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Models
{
    [Collection("SettingMaster")]
    public class mdSettingMaster : Entity
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
        public string StringValue1 { get; set; } = "";
        public string StringValue2 { get; set; } = "";        
        public int IntValue1 { get; set; }
        public int IntValue2 { get; set; }
        public double DoubleValue1 { get; set; }
        public double DoubleValue2 { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
    }
}
