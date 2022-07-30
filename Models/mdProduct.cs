using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Models
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
        public string VendorLinks { get; set; } = "";
        public string QALinks { get; set; } = "";
        public string LogoID { get; set; } = "";
        public string VendorLogoID { get; set; } = "";
        public string BuyPolicy { get; set; } = "";
        public string Notes { get; set; } = "";
        public string SaleImageID { get; set; } = "";
        public string FlashCardID { get; set; } = "";
        public int Duration { get; set; }
        public string DurationUnit { get; set; } = "";
        public List<SpecificationModel> Specifications { get; set; } = new List<SpecificationModel>();
        public List<BenefitModel> Benefits { get; set; } = new List<BenefitModel>();
        public List<SalePackageModel> SalePackages { get; set; } = new List<SalePackageModel>();
        
        //
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

    public class BenefitModel
    {
        public string ID { get; set; } = "";
        public string Name { get; set; } = "";
        public double BenefitAmount { get; set; }
        public double FeeRate { get; set; }
        public double FeeAmount { get; set; }
        public string Notes { get; set; } = "";
        public List<BenefitItemModel> BenefitItems { get; set; } = new List<BenefitItemModel>();
    }

    public class BenefitItemModel
    {
        public string ID { get; set; } = "";
        public string Name { get; set; } = "";
        public string Specification { get; set; } = "";
        public string Conditions { get; set; } = "";
        public DateTime EffStartDate { get; set; }
        public DateTime EffEndDate { get; set; }
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public int MaxClaimCount { get; set; }
        public double MaxClaimAmount { get; set; }
        public double MaxTotalClaimAmount { get; set; }
        public string Notes { get; set; } = "";
    }

    public class SalePackageModel
    {
        public string PackageID { get; set; } = "";
        public string PackageName { get; set; } = "";
        public string TargetID { get; set; } = "";
        public string TargetName { get; set; } = "";
        public double UnitPrice { get; set; }
        public double BenefitAmount { get; set; }
        public string Notes { get; set; } = "";
    }

}
