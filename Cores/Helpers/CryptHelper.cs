using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cores.Helpers
{
    /// <summary>
    /// Lớp cung cấp các hàm mã hóa dữ liệu
    /// </summary>
    public partial class CryptHelper
    {
        /// <summary>
        /// Mã hóa chuỗi dữ liệu
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Encrypt(string key, string data)
        {
            try
            {
                data = data.Trim();
                byte[] keydata = Encoding.ASCII.GetBytes(key);
                string md5String = BitConverter.ToString(new
                MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-", "").ToLower();
                byte[] tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));
                TripleDES tripdes = TripleDESCryptoServiceProvider.Create();
                tripdes.Mode = CipherMode.ECB;
                tripdes.Key = tripleDesKey;
                tripdes.GenerateIV();
                MemoryStream ms = new MemoryStream();
                CryptoStream encStream = new CryptoStream(ms, tripdes.CreateEncryptor(),
                CryptoStreamMode.Write);
                encStream.Write(Encoding.ASCII.GetBytes(data), 0,
                Encoding.ASCII.GetByteCount(data));
                encStream.FlushFinalBlock();
                byte[] cryptoByte = ms.ToArray();
                ms.Close();
                encStream.Close();
                return Convert.ToBase64String(cryptoByte, 0, cryptoByte.GetLength(0)).Trim();
            }
            catch { }
            return data;
        }
        /// <summary>
        /// Giải mã chuỗi dữ liệu
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Decrypt(string key, string data)
        {
            try
            {
                byte[] keydata = Encoding.ASCII.GetBytes(key);
                string md5String = BitConverter.ToString(new
                MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-", "").ToLower();
                byte[] tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));
                TripleDES tripdes = TripleDESCryptoServiceProvider.Create();
                tripdes.Mode = CipherMode.ECB;
                tripdes.Key = tripleDesKey;
                byte[] cryptByte = Convert.FromBase64String(data);
                MemoryStream ms = new MemoryStream(cryptByte, 0, cryptByte.Length);
                ICryptoTransform cryptoTransform = tripdes.CreateDecryptor();
                CryptoStream decStream = new CryptoStream(ms, cryptoTransform,
                CryptoStreamMode.Read);
                StreamReader read = new StreamReader(decStream);
                return (read.ReadToEnd());
            }
            catch { }
            return data;
        }
        /// <summary>
        /// Hàm mã hóa chuỗi
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <param name="key"></param>
        /// <param name="useHashing"></param>
        /// <returns></returns>
        public static string Encrypt(string toEncrypt, string key, bool useHashing)
        {
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

                if (useHashing)
                {
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                }
                else
                    keyArray = UTF8Encoding.UTF8.GetBytes(key);

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch { }
            return toEncrypt;
        }
        /// <summary>
        /// Hàm giải mã chuỗi
        /// </summary>
        /// <param name="toDecrypt"></param>
        /// <param name="key"></param>
        /// <param name="useHashing"></param>
        /// <returns></returns>
        public static string Decrypt(string toDecrypt, string key, bool useHashing)
        {
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

                if (useHashing)
                {
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                }
                else
                    keyArray = UTF8Encoding.UTF8.GetBytes(key);

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch { }
            return toDecrypt;
        }
        /// <summary>
        /// Mã hóa MD5 cho chuỗi text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string EncodeMD5(string text)
        {
            try
            {
                MD5 md5 = MD5CryptoServiceProvider.Create();
                byte[] dataMd5 = md5.ComputeHash(Encoding.Default.GetBytes(text));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < dataMd5.Length; i++)
                    sb.AppendFormat("{0:x2}", dataMd5[i]);
                return sb.ToString();
            }
            catch { }
            return text;
        }

        public static string sha256(string randomString)
        {
            try
            {
                var crypt = new SHA256Managed();
                string hash = String.Empty;
                byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
                foreach (byte theByte in crypto)
                {
                    hash += theByte.ToString("x2");
                }
                return hash;
            }
            catch
            {
                return "";
            }           
        }

        public static byte[] EncryptStringToBytesAES128(string plainText, byte[] Key)
        {
            byte[] encrypted;
            byte[] IV;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;

                aesAlg.GenerateIV();
                IV = aesAlg.IV;

                aesAlg.Mode = CipherMode.CBC;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            var combinedIvCt = new byte[IV.Length + encrypted.Length];
            Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
            Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);

            // Return the encrypted bytes from the memory stream.
            return combinedIvCt;
        }

        public static string DecryptStringFromBytesAES128(byte[] cipherTextCombined, byte[] Key)
        {
            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;

                byte[] IV = new byte[aesAlg.BlockSize / 8];
                byte[] cipherText = new byte[cipherTextCombined.Length - IV.Length];

                Array.Copy(cipherTextCombined, IV, IV.Length);
                Array.Copy(cipherTextCombined, IV.Length, cipherText, 0, cipherText.Length);

                aesAlg.IV = IV;

                aesAlg.Mode = CipherMode.CBC;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

    }
}
