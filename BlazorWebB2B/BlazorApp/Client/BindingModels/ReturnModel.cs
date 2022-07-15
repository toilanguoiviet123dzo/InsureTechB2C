using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Client.BindingModels
{
    public class RepairEstimation_ReturnModel
    {
        public int UpdMode { get; set; }
        public double EstRepairPrice { get; set; }
        public double DealRepairPrice { get; set; }
        public double AprRepairPrice { get; set; }
        public double EstVAT { get; set; }
        public double DealVAT { get; set; }
        public double AprVAT { get; set; }
    }
}
