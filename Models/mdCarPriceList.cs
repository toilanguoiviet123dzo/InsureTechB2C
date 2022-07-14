using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Models
{
    [Collection("CarPriceList")]
    public class mdCarPriceList : Entity
    {
        public string VendorID { get; set; } = "";
        public string ProductID { get; set; } = "";
        public string BusinessType { get; set; } = "";
        public string BusinessTypeName { get; set; } = "";
        public string CarType { get; set; } = "";
        public string CarTypeName { get; set; } = "";
        public bool BySeat { get; set; } = true;
        public double FromSeatCount { get; set; }
        public double ToSeatCount { get; set; }
        public double FromTonage { get; set; }
        public double ToTonage { get; set; }
        public double UnitPrice { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
    }

}
