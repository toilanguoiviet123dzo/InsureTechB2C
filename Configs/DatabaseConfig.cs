using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Configs
{
    public class DatabaseConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string DBName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConnectionString { get; set; }
        public string MasterDBName { get; set; }
        public string MasterConnectionString { get; set; }
    }
}
