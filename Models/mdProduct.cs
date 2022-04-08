using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Server.Models
{
    [Collection("Product")]
    public class mdProduct : Entity
    {
        public string ProductType { get; set; } = "";
        public string ProductID { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string VendorID { get; set; } = "";
        public string VendorName { get; set; } = "";
        public double InsureAmount { get; set; }
        public double UnitPrice { get; set; }
        public double TaxRate { get; set; }
        public bool IsIncludeVAT { get; set; }
        public int Duration { get; set; }
        public string DurationUnit { get; set; } = "";
        public string VendorLinks { get; set; } = "";
        public string QALinks { get; set; } = "";
        public string LogoID { get; set; } = "";
        public string VendorLogoID { get; set; } = "";
        public string Notes { get; set; } = "";
        public string BuyPolicy { get; set; } = "";
        public List<SpecificationModel> Specifications { get; set; } = new List<SpecificationModel>();
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
    }

    public class SpecificationModel
    {
        public int DspOrder { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string RecNo { get; set; } = "";
    }

}
