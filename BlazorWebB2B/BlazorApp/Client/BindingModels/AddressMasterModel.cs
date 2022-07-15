using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.BindingModels
{
    public class CityModel
    {
        public string ID { get; set; } = "";
        public int DspOrder { get; set; }
        public string CityID { get; set; } = "";
        public string CityName { get; set; } = "";
        public string CityNameEN { get; set; } = "";
        public List<DistrictModel> Districts { get; set; } = new List<DistrictModel>();
        public int UpdMode { get; set; }
    }

    public class DistrictModel
    {
        public int DspOrder { get; set; }
        public string DistrictID { get; set; } = "";
        public string DistrictName { get; set; } = "";
        public string DistrictNameEN { get; set; } = "";
        public List<WardModel> Wards { get; set; } = new List<WardModel>();
        public int UpdMode { get; set; }
    }

    public class WardModel
    {
        public int DspOrder { get; set; }
        public string WardID { get; set; } = "";
        public string WardName { get; set; } = "";
        public string WardNameEN { get; set; } = "";
        public int UpdMode { get; set; }
    }
}
