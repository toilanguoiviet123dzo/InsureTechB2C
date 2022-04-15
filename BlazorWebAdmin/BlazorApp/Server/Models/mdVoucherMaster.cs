using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Server.Models
{
    [Collection("VoucherMaster")]
    public class mdVoucherMaster : Entity
    {
        public string VoucherCode { get; set; } = "";
        public string CurrentVoucherNo { get; set; } = "";
        public string MinVoucherNo { get; set; } = "";
        public string MaxVoucherNo { get; set; } = "";
        public string Notes { get; set; } = "";
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
    }
}
