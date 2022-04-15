using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Server.Models
{
    [Collection("OptionListHeader")]
    public class mdOptionListHeader : Entity
    {
        public string ListCode { get; set; } = "";
        public string ListName { get; set; } = "";
        public string Description { get; set; } = "";
        public int UpdMode { get; set; }        
    }
}
