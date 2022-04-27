using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.BindingModels
{
    public class DiscountCodeModel
    {
        public string ID { get; set; } = "";
        [Required(ErrorMessage = "Bắt buộc nhập.")]
        public string DiscountCode { get; set; } = "";
        [Required(ErrorMessage = "Bắt buộc nhập.")]
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
        public bool Enabled { get; set; }
        public bool IsPublic { get; set; }
        public string PurchaseLink { get; set; } = "";
        public string QrCode { get; set; } = "";
        public string Notes { get; set; } = "";
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
        //Row mode
        public bool RowMode_View { get; set; } = false;
        public bool RowMode_Edit { get; set; } = true;
        public bool RowMode_Delete { get; set; } = true;
    }
}