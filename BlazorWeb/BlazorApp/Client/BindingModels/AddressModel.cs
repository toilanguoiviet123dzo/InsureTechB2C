using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.BindingModels
{
    public class AddressModel
    {
        public string ID { get; set; } = "";
        public int DspOrder { get; set; }
        public int Level { get; set; }
        public string CityID { get; set; } = "";
        public string DistrictID { get; set; } = "";
        [Required(ErrorMessage = "Bắt buộc nhập.")]
        public string ItemID { get; set; } = "";
        [Required(ErrorMessage = "Bắt buộc nhập.")]
        public string ItemName { get; set; } = "";
        public string ItemNameEN { get; set; } = "";
        public int UpdMode { get; set; }
        //Row mode
        public bool RowMode_View { get; set; } = false;
        public bool RowMode_Edit { get; set; } = true;
        public bool RowMode_Delete { get; set; } = true;
    }
}
