using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Models
{
    [Collection("UserAccount")]
    public class mdUserAccount : Entity
    {
        public string UserID { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string Fullname { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string RoleID { get; set; } = "";
        public string RoleName { get; set; } = "";
        public string MerchantID { get; set; } = "";
        public string MerchantName { get; set; } = "";
        public string UserCode { get; set; } = "";
        public string RefCode { get; set; } = "";
        public int RankLevel { get; set; }
        public bool Status { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
    }
}
