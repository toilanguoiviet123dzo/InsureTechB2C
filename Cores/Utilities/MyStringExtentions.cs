using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Cores.Utilities
{
    public static class MyStringExtentions
    {
        #region String manipulation
        public static string Left(this string s, int nCount)
        {
            return s.Substring(0, nCount);
        }

        public static string Right(this string s, int nCount)
        {
            return s.Substring(s.Length - nCount, nCount);
        }

        public static string Mid(this string s, int nIndex, int nCount)
        {
            return s.Substring(nIndex, nCount);
        }
        /// <summary>
        /// Trích ra từ một chuỗi các giá trị số
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetNumericString(this string s)
        {
            string sTemp = "";
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsDigit(s[i]))
                {
                    sTemp += s[i];
                }
            }
            return sTemp;
        }
        /// <summary>
        /// Kiểm tra xem 1 chuỗi có phải là chuỗi số hay không
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNumericString(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }
            else
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (!char.IsDigit(s[i]))
                        return false;
                }
                return true;
            }
        }
        #endregion

        #region conversion
        public static string Beautify_VnName(this string fullname)
        {
            string ret = "";
            string[] arr = fullname.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in arr)
            {
                ret += char.ToUpper(item[0]) + item.Substring(1) + " ";
            }
            //
            return ret.Trim();
        }


        private static readonly string[] VietnameseSigns = new string[]
        {
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ",
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ",
            "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ",
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ",
            "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ",
            "ÍÌỊỈĨ",
            "đ",
            "Đ",
            "ýỳỵỷỹ",
            "ÝỲỴỶỸ"
        };
        public static string RemoveVietnameseSign(this string s)
        {
            //Replace sign
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                    s = s.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
            }
            //
            return s;
        }
        public static string Base64Encode(string plainText)
        {
            try
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return System.Convert.ToBase64String(plainTextBytes);
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static int ToInt(this string s)
        {
            int numValue = 0;
            int.TryParse(s, out numValue);
            return numValue;
        }
        public static double ToDouble(this string s)
        {
            double numValue = 0;
            double.TryParse(s, out numValue);
            return numValue;
        }
        public static decimal ToDecimal(this string s)
        {
            decimal numValue = 0;
            decimal.TryParse(s, out numValue);
            return numValue;
        }
        #endregion

        #region Encoding
        private static string _key = "Pehera5963$$@@!!";
        public static string EncryptMD5(this string inputString)
        {
            {
                using (var md5 = new MD5CryptoServiceProvider())
                {
                    using (var tdes = new TripleDESCryptoServiceProvider())
                    {
                        tdes.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(_key));
                        tdes.Mode = CipherMode.ECB;
                        tdes.Padding = PaddingMode.PKCS7;

                        using (var transform = tdes.CreateEncryptor())
                        {
                            byte[] textBytes = Encoding.UTF8.GetBytes(inputString);
                            byte[] bytes = transform.TransformFinalBlock(textBytes, 0, textBytes.Length);
                            return Convert.ToBase64String(bytes, 0, bytes.Length);
                        }
                    }
                }
            }
        }

        public static string DecryptMD5(this string inputString)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(_key));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateDecryptor())
                    {
                        byte[] cipherBytes = Convert.FromBase64String(inputString);
                        byte[] bytes = transform.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return Encoding.UTF8.GetString(bytes);
                    }
                }
            }
        }
        /// <summary>
        /// Mã hóa MD5
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Md5(this string s)
        {
            try
            {
                MD5 md5 = MD5.Create();
                byte[] dataMd5 = md5.ComputeHash(Encoding.Default.GetBytes(s));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < dataMd5.Length; i++)
                    sb.AppendFormat("{0:x2}", dataMd5[i]);
                return sb.ToString();
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Md5UTF8(this string s)
        {
            try
            {
                MD5 md5 = MD5.Create();
                byte[] dataMd5 = md5.ComputeHash(Encoding.UTF8.GetBytes(s));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < dataMd5.Length; i++)
                    sb.AppendFormat("{0:x2}", dataMd5[i]);
                return sb.ToString();
            }
            catch
            {
                return "";
            }
        }

        public static string hmacSHA256(string data, string key)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes(key)))
            {
                byte[] arr = hmac.ComputeHash(Encoding.ASCII.GetBytes(data));
                return BitConverter.ToString(arr).Replace("-", "").ToLower();
            }
        }
        #endregion

        #region Validation
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsPhoneNumber(this string s)
        {
            return Regex.IsMatch(s, @"(84|0[3|5|7|8|9])+([0-9]{8})\b");
        }
        /// <summary>
        /// Bỏ các ký tự định dạng HTML ra khỏi chuỗi
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string StripHtml(this string html)
        {
            try
            {
                if (html == null || html == string.Empty)
                    return string.Empty;

                return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty);
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// Kiểm tra email hợp lệ
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        public static bool IsEmail(this string Email)
        {
            if (string.IsNullOrEmpty(Email))
                return false;
            return Regex.IsMatch(Email, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
        }
        /// <summary>
        /// Kiểm tra xem mật khẩu có hợp lệ hay không
        /// </summary>
        /// <param name="Password"></param>
        /// <returns></returns>
        public static bool IsValidPasswordEasy(this string Password)
        {
            if (string.IsNullOrEmpty(Password)) //Mật khẩu rỗng
            {
                return false;
            }
            if (Password.Length < 6)    //Độ dài nhỏ hơn 6
            {
                return false;
            }

            //Danh sách các password cấm sử dụng
            string[] _DeniedPasswords = new string[]
            {
                "123456",
                "1234567",
                "876543",
                "abcdef",
                "abc1234",
                "abcd123",
                "abcd12",
                "password"
            };
            if (_DeniedPasswords.Contains(Password.ToLower()))
            {
                return false;
            }

            return true;
        }
        #endregion

    }
}
