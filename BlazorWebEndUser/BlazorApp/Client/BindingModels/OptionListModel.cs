using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.BindingModels
{
    public class OptionListModel
    {
        public string ID { get; set; } = "";
        public string ListCode { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public int IntCode { get; set; }
        public double DoubleCode { get; set; }
        [Required(ErrorMessage = "Bắt buộc nhập.")]
        public string ItemName { get; set; } = "";
        public string DspOrder { get; set; } = "";
        public string ExtraInfo1 { get; set; } = "";
        public string ExtraInfo2 { get; set; } = "";
        public string ExtraInfo3 { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
        //Row mode
        public bool RowMode_View { get; set; } = false;
        public bool RowMode_Edit { get; set; } = true;
        public bool RowMode_Delete { get; set; } = true;
    }
}
