using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.BindingModels
{
    public class DiscountCodeModel
    {
        public string ID { get; set; } = "";
        public string DiscountCode { get; set; } = "";
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
        public bool Checked { get; set; }
        public int ExpiredIn { get; set; }
    }
}
