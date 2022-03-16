using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.BindingModels
{
    public class UserAccountModel
    {
        public string ID { get; set; } = "";
        public string UserID { get; set; } = "";
        [Required(ErrorMessage = "Bắt buộc nhập.")]
        public string UserName { get; set; } = "";
        [Required(ErrorMessage = "Bắt buộc nhập.")]
        public string Password { get; set; } = "";
        [Required(ErrorMessage = "Bắt buộc nhập.")]
        public string Fullname { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string RoleID { get; set; } = "";
        public string RoleName { get; set; } = "";
        public string BrancheID { get; set; } = "";
        public string BrancheName { get; set; } = "";
        public string LogisticCompanyID { get; set; } = "";
        public string LogisticCompanyName { get; set; } = "";
        public string RepairCompanyID { get; set; } = "";
        public string RepairCompanyName { get; set; } = "";
        public string ApproveAcountID { get; set; } = "";
        public int ApproveLevel { get; set; }
        public string ApproveLevelName { get; set; }
        public int DocumentLevel { get; set; }
        public string DocumentLevelName { get; set; }
        public bool Status { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
        //Row mode
        public bool RowMode_View { get; set; } = false;
        public bool RowMode_Edit { get; set; } = true;
        public bool RowMode_Delete { get; set; } = true;
    }
}
