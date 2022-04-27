using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.BindingModels
{
    public class ProductModel
    {
        public string ID { get; set; } = "";
        [Required(ErrorMessage = "Bắt buộc nhập")]
        public string ProductID { get; set; } = "";
        public string ProductType { get; set; } = "";
        [Required(ErrorMessage = "Bắt buộc nhập")]
        public string ProductName { get; set; } = "";
        public string VendorID { get; set; } = "";
        public string VendorName { get; set; } = "";
        public double InsureAmount { get; set; }
        public double UnitPrice { get; set; }
        public double ExtraPrice { get; set; }
        public string ExtraPriceName { get; set; } = "";
        public double TaxRate { get; set; }
        public bool IsIncludeVAT { get; set; }
        public int Duration { get; set; }
        public string DurationUnit { get; set; } = "";
        public List<SpecificationModel> Specifications { get; set; } = new List<SpecificationModel>();
        public string VendorLinks { get; set; } = "";
        public string QALinks { get; set; } = "";
        public string LogoID { get; set; } = "";
        public byte[] LogoContent { get; set; } = new byte[] { };
        public string VendorLogoID { get; set; } = "";
        public byte[] VendorLogoContent { get; set; } = new byte[] { };
        public string BuyPolicy { get; set; } = "";
        public bool BuyExtraPrice { get; set; }
        public string Notes { get; set; } = "";
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
        //
        //Row mode
        public bool RowMode_View { get; set; } = false;
        public bool RowMode_Edit { get; set; } = true;
        public bool RowMode_Delete { get; set; } = true;
    }

    public class SpecificationModel
    {
        public int DspOrder { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string RecNo { get; set; } = "";
        //Row mode
        public bool RowMode_View { get; set; } = false;
        public bool RowMode_Edit { get; set; } = true;
        public bool RowMode_Delete { get; set; } = true;
    }
}
