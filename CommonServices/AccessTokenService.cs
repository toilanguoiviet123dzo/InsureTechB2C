using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gosu.Common;

namespace Gosu.Grpc.Authentication
{
    public static class AccessTokenService
    {
        public static string GenAccessToken(string UserName,
                                            string ClientID,
                                            DateTime Timeout)
        {
            //build
            var strTimeOut = Timeout.ToString("yyMMddHHmm");
            string accessToken = $"{ClientID}|7|{UserName}|8|123dzo|9|{strTimeOut}";

            //Return
            return accessToken.EncryptMD5();
        }

        public static string RefreshAccessToken(string accessToken,
                                                DateTime Timeout)
        {
            //Parse old token
            var decryptedAccessToken = accessToken.DecryptMD5();
            var strArr = decryptedAccessToken.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strArr == null || strArr.Length != 7) return accessToken;

            //Refresh new time out
            var strTimeOut = Timeout.ToString("yyMMddHHmm");
            string refreshAccessToken = $"{strArr[0]}|7|{strArr[2]}|8|789dzo|9|{strTimeOut}";

            //Return
            return refreshAccessToken.EncryptMD5();
        }
        //
        public static bool Check_AccessToken(string userName, string clientID, string accessToken)
        {
            //Validate
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(accessToken)) return false;

            //Parse token to check
            string DecryptedAccessToken = accessToken.DecryptMD5();
            var strArray = DecryptedAccessToken.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strArray == null || strArray.Length != 7) return false;

            //Check format
            if (strArray[1] != "7" || strArray[3] != "8" || strArray[5] != "9") return false;

            //Check UserName, clientID
            // || strArray[2] != userName
            if (strArray[0] != clientID) return false;

            //Check timeout
            if (strArray[6].CompareTo(DateTime.UtcNow.ToString("yyMMddHHmm")) < 0) return false;

            //OK
            return true;
        }
        //

    }
}
