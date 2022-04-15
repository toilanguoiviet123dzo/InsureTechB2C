using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Server.Models
{    
    [Collection("AppLog")]
    public class mdAppLog : Entity
    {
        public int LogLevel { get; set; }
        public string Class { get; set; } = "";
        public string Method { get; set; } = "";
        public string Step { get; set; } = "";
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; } = "";
        public DateTime CreatedOn { get; set; }
    }
}
