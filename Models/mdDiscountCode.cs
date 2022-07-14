using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Models
{
    [Collection("DiscountCode")]
    public class mdDiscountCode : Entity
    {
        public string DiscountCode { get; set; } = "";
        public string DiscountName { get; set; } = "";
        public string Descriptions { get; set; } = "";
        public string ProductID { get; set; } = "";
        public string ProductName { get; set; } = "";
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double DiscountRate { get; set; }
        public double DiscountAmount { get; set; }
        public double UserMaxQty { get; set; }
        public double TotalMaxQty { get; set; }
        public double UsedQty { get; set; }
        public bool Enabled { get; set; } = true;
        public bool IsPublic { get; set; } = true;
        public string PurchaseLink { get; set; } = "";
        public string QrCode { get; set; } = "";
        public string Notes { get; set; } = "";
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
    }
}
