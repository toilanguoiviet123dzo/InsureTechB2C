using BlazorApp.Client.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cores.GrpcClient.Authentication
{
    public static class WebUserCredential
    {
        //Program
        public static ProgramInfo Program = new ProgramInfo("", "");
        //
        public static bool IsAuthenticated = false;
        //
        public static string Username = "";
        public static string Fullname = "";
        public static string PhoneNo = "0375554350";
        public static string RoleID = "";
        public static string MerchantID = "InsurTech";
        public static string AccessToken = "";
        public static string ApiKey = "apiKey5963";
        public static string GrantedPages = "";
        public static bool IsDevelopmentMode = false;
    }
}
