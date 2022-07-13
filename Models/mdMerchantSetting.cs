using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Server.Models
{
    [Collection("MerchantSetting")]
    public class mdMerchantSetting : Entity
    {
        public string MerchantID { get; set; } = "";
        public string MerchantName { get; set; } = "";
        public string ProductID { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string VendorID { get; set; } = "";
        public double BonusRate { get; set; }
        public DateTime EffSttDate { get; set; }
        public string Notes { get; set; } = "";
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
    }
}
