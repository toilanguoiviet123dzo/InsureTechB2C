using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cores.Utilities;

namespace Server.Common
{
    public static class MyTokenService
    {
        public static string GenInitOrderToken(string transactionID)
        {
            //Token
            string token = $"1234fgh*&^^%6556789{transactionID}987dfgh*&^^%654321";

            //Return
            return token.hmacSHA256();
        }

        public static string GenFinishOrderToken(string transactionID)
        {
            //Token
            string token = $"1234fgh*&^^%65fghfg789{transactionID}987*&^fgh*&^^%654321";

            //Return
            return token.hmacSHA256();
        }

        public static bool Check_InitOrderToken(string inputHashedToken, string transactionID)
        {
            try
            {
                //Token
                string token = $"1234fgh*&^^%6556789{transactionID}987dfgh*&^^%654321";

                //Hash
                string hashedToken = token.hmacSHA256();

                //
                return hashedToken == inputHashedToken;
            }
            catch
            {
                return false;
            }
        }

        public static bool Check_FinishOrderToken(string inputHashedToken, string transactionID)
        {
            try
            {
                //Token
                string token = $"1234fgh*&^^%65fghfg789{transactionID}987*&^fgh*&^^%654321";

                //Hash
                string hashedToken = token.hmacSHA256();

                //
                return hashedToken == inputHashedToken;
            }
            catch
            {
                return false;
            }
        }

    }
}
