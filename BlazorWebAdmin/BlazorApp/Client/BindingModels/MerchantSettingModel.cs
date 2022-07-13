using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.BindingModels
{
    public class MerchantSettingModel
    {
        public string ID { get; set; } = "";
        public string MerchantID { get; set; } = "";
        public string MerchantName { get; set; } = "";
        public string ProductID { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string VendorID { get; set; } = "";
        public double BonusRate { get; set; }
        public DateTime EffSttDate { get; set; }
        public string Notes { get; set; } = "";
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
        //Row mode
        public bool RowMode_View { get; set; } = false;
        public bool RowMode_Edit { get; set; } = true;
        public bool RowMode_Delete { get; set; } = true;
    }
}
