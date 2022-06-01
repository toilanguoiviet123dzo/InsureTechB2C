using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cores.Utilities
{
    public static class MyCodeGenerator
    {
        #region Cope generation
        public static string GenOTP()
        {
            return RandomCode(0, 6);
        }

        public static string GenAccessToken(int UserDBCount,
                                            string UserName,
                                            string RoleID,
                                            string DeviceID,
                                            string GameID)
        {
            //X: Cal User DB index count
            if (UserDBCount == 0) UserDBCount = 1;
            string DBIndex = (UserName.GetHashCode() % UserDBCount).ToString();

            //Y
            string StrToEncrypt = RoleID + UserName + DeviceID + GameID + Guid.NewGuid().ToString();

            //Return
            return DBIndex + MD5Crypto.Encrypt(StrToEncrypt);
        }

        public static string GenRecNo()
        {
            return DateTime.Now.ToString("yyMMddHHmmssfff") + RandomCode(0, 2);
        }
        public static string GenTransactionID()
        {
            return DateTime.Now.ToString("yyMMddHHmmssfff") + RandomCode(0, 2);
        }
        public static string GenActivationCode()
        {
            return DateTime.Now.ToString("yyMMddHHmmssfff") + RandomCode(5, 5);
        }
        public static string GenResourceID()
        {
            return DateTime.Now.ToString("yyMMddHHmmssfff") + RandomCode(0, 3);
        }

        public static string GenCustomerID()
        {
            return DateTime.Now.ToString("yyMMddHHmmssfff") + RandomCode(0, 1);
        }
        #endregion

        #region Random
        // Instantiate random number generator.  
        private static readonly Random _random = new Random();
        // Generates a random string with a given size.    
        public static string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.  

            // char is a single Unicode character  
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length=26  

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }

        // Generates a random number within a range.      
        public static int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        // Generates a random password.  
        // 4-LowerCase + 4-Digits
        public static string RandomCode(int letterSize, int numberSize, bool isLowerCase = true)
        {
            var passwordBuilder = new StringBuilder();

            // letterSize-Letters lower case   
            if (letterSize > 0)
            {
                passwordBuilder.Append(RandomString(letterSize, isLowerCase));
            }

            // numberSize-Digits 
            if (numberSize > 0)
            {
                passwordBuilder.Append(RandomNumber((int)Math.Pow(10, numberSize - 1), (int)Math.Pow(10, numberSize)) - 1);
            }
            //
            return passwordBuilder.ToString();
        }
        #endregion
        //
    }
}
