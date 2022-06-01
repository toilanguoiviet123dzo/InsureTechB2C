using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Server.Models
{
    [Collection("UserAccountQueue")]
    public class mdUserAccountQueue : Entity
    {
        public bool IsDone { get; set; } = false;
        public string ActivationCode { get; set; } = "";
        public DateTime ActivationOn { get; set; }
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
    }
}
