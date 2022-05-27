using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Server.Models
{
    [Collection("OptionList")]
    public class mdOptionList : Entity
    {
        public string ListCode { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public int IntCode { get; set; }
        public double DoubleCode { get; set; }
        public string ItemName { get; set; } = "";                
        public string DspOrder { get; set; } = "";
        public string ExtraInfo1 { get; set; } = "";
        public string ExtraInfo2 { get; set; } = "";
        public string ExtraInfo3 { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
    }
}
