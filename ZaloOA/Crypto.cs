using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ZaloOA
{
    public class Crypto
    {
        public static string SHA256(string stringToHash)
        {
            var crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(stringToHash));
            return Convert.ToBase64String(crypto);
        }
    }
}
