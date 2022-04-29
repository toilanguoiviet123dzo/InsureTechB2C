using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Server.Models
{
    [Collection("CarPriceList")]
    public class mdCarPriceList : Entity
    {
        public string ProductID { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string BusinessType { get; set; } = "";
        public string BusinessTypeName { get; set; } = "";
        public string PriceType { get; set; } = ""; //1: by seat  2: by tonage
        public double SeatCount { get; set; }
        public double Tonage { get; set; }
        public double UnitPrice { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
    }

}
