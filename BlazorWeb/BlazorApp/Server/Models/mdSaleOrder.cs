using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Server.Models
{
    [Collection("SaleOrder")]
    public class mdSaleOrder : Entity
    {
        //Order info
        public string OrderID { get; set; } = "";
        public string OwnerFullname { get; set; } = "";
        public string LicensePlate { get; set; } = "";
        public string CusFullname { get; set; } = "";
        public string CusPhone { get; set; } = "";
        public string CusEmail { get; set; } = "";
        public DateTime EffectiveSttDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }
        public string CityID { get; set; } = "";
        public string CityName { get; set; } = "";
        public string DistrictID { get; set; } = "";
        public string DistrictName { get; set; } = "";
        public string WardID { get; set; } = "";
        public string WardName { get; set; } = "";
        public string Address { get; set; } = "";
        //Product
        public string ProductID { get; set; } = "";
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double Amount { get; set; }
        public bool IsIncludeVAT { get; set; }
        public double TaxRate { get; set; }
        public double TaxAmount { get; set; }
        //Status
        public bool IsMakeOrder { get; set; }
        public bool IsPayed { get; set; }
        public bool IsIssueCertificate { get; set; }
        //
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
    }



}
