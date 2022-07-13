using System;
using Cores.Utilities;

namespace BlazorApp.Server.Services
{
    public static class AccessTokenService
    {
        private static int UserTimeOutSpan = 24; //hour
        //
        public static string Generate_AccessToken(string userName, string roleID)
        {
            string accessToken = userName + "|" + roleID + "|" + DateTime.UtcNow.AddHours(UserTimeOutSpan).ToString("yyMMddHHmm");
            return accessToken.EncryptMD5();
        }
        //
        public static bool Check_AccessToken(string userName, string roleID, string accessToken)
        {
            //Validate
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(roleID) || string.IsNullOrWhiteSpace(accessToken)) return false;

            //Parse token to check
            string DecryptedAccessToken = accessToken.DecryptMD5();
            var strArray = DecryptedAccessToken.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strArray == null || strArray.Length != 3) return false;

            //Check UserName, RoleID
            if (strArray[0] != userName || strArray[1] != roleID) return false;

            //Check timeout
            if (strArray[2].CompareTo(DateTime.UtcNow.ToString("yyMMddHHmm")) < 0) return false;

            //OK
            return true;
        }
        //

    }
}
