using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Models
{
    [Collection("Policy")]
    public class mdPolicy : Entity
    {
        public string OrderID { get; set; } = "";
        public string HolderID { get; set; } = "";
        public string PolicyNo { get; set; } = "";
        public DateTime PolicyDate { get; set; }
        public string Fullname { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public string Sex { get; set; } = "";
        public string CusCitizenID { get; set; } = "";
        public string ProductType { get; set; } = "";
        public string VendorID { get; set; } = "";
        public string VendorName { get; set; } = "";
        public string ProductID { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string SalePackageID { get; set; } = "";
        public string SalePackageName { get; set; } = "";
        public string TarketID { get; set; } = "";
        public string TarketName { get; set; } = "";
        public DateTime EffectiveSttDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }
        public int Duration { get; set; }
        public string DurationUnit { get; set; } = "";
        public double FeeAmount { get; set; }
        public double BenefitAmount { get; set; }
        public bool IsIncludeVAT { get; set; }
        public double TaxRate { get; set; }
        public double TaxAmount { get; set; }
        public string Currency { get; set; } = "VND";
        public double ExchangeRate { get; set; } = 1;
        public string CertificateLink { get; set; } = "";
        //
        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;
    }

}
