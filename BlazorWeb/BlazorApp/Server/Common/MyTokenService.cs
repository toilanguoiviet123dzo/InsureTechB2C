using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cores.Utilities;

namespace BlazorApp.Server.Common
{
    public static class MyTokenService
    {
        private static int Duration = 40;
        public static string GenInitOrderToken(string transactionID)
        {
            //build
            var strTimeOut = DateTime.UtcNow.AddMinutes(Duration).ToString("yyMMddHHmm");
            string token = $"123dzo|7|{transactionID}|8|456dzo|9|{strTimeOut}";

            //Return
            return token.EncryptMD5();
        }

        public static string GenFinishOrderToken(string transactionID)
        {
            //build
            var strTimeOut = DateTime.UtcNow.AddMinutes(Duration).ToString("yyMMddHHmm");
            string token = $"dzo123|10|{transactionID}|11|dzo456|12|{strTimeOut}";

            //Return
            return token.EncryptMD5();
        }

        public static bool Check_InitOrderToken(string token, string transactionID)
        {
            //Validate
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(transactionID)) return false;

            //Parse token to check
            string DecryptedAccessToken = token.DecryptMD5();
            var strArray = DecryptedAccessToken.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strArray == null || strArray.Length != 7) return false;

            //Check format
            if (strArray[1] != "7" || strArray[3] != "8" || strArray[5] != "9") return false;

            //Check TransactionID
            if (strArray[2] != transactionID) return false;

            //Check timeout
            if (strArray[6].CompareTo(DateTime.UtcNow.ToString("yyMMddHHmm")) < 0) return false;

            //OK
            return true;
        }

        public static bool Check_FinishOrderToken(string token, string transactionID)
        {
            //Validate
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(transactionID)) return false;

            //Parse token to check
            string DecryptedAccessToken = token.DecryptMD5();
            var strArray = DecryptedAccessToken.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strArray == null || strArray.Length != 7) return false;

            //Check format
            if (strArray[1] != "10" || strArray[3] != "11" || strArray[5] != "12") return false;

            //Check TransactionID
            if (strArray[2] != transactionID) return false;

            //Check timeout
            if (strArray[6].CompareTo(DateTime.UtcNow.ToString("yyMMddHHmm")) < 0) return false;

            //OK
            return true;
        }

    }
}
