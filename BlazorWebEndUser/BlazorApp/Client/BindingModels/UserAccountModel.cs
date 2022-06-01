using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.BindingModels
{
    public class UserAccountModel
    {
        public string ID { get; set; } = "";
        public string ActivationCode { get; set; } = "";
        public string UserID { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string Fullname { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string RoleID { get; set; } = "";
        public string RoleName { get; set; } = "";
        public string RefUserID { get; set; } = "";
        public int RankLevel { get; set; }
        public bool Status { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
        //Row mode
        public bool RowMode_View { get; set; } = false;
        public bool RowMode_Edit { get; set; } = true;
        public bool RowMode_Delete { get; set; } = true;
    }
}
