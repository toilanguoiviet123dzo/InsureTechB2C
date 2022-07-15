using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cores.GrpcClient.Authentication
{
    public class AuthenticationResponseModel
    {
        public string UserName { get; set; }
        public string Fullname { get; set; }
        public int RoleID { get; set; }
    }
}
