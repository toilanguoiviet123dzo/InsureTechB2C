using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.BindingModels
{
    public class CarPriceModel
    {
        public string ID { get; set; } = "";
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
        //
        //Row mode
        public bool RowMode_View { get; set; } = false;
        public bool RowMode_Edit { get; set; } = true;
        public bool RowMode_Delete { get; set; } = true;
    }

}
