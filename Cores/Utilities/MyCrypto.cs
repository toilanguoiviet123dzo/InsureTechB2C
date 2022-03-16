using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cores.Utilities
{
    public static class MD5Crypto
    {
        private static string _key = "Pehera5963$$@@!!";
        public static string Encrypt(string inputString)
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

        public static string Decrypt(string inputString)
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
        //
        /// <summary>
        /// Mã hóa MD5
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Md5(string s)
        {
            try
            {
                MD5 md5 = MD5CryptoServiceProvider.Create();
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
        //
    }

    /// <summary>
    /// HMACSHA25: Computes a Hash-based Message Authentication Code (HMAC) by using the SHA256 hash function.
    /// </summary>
    public static class HMACSHA256Crypto
    {
        private static byte[] HashHMAC(byte[] key, byte[] message)
        {
            var hash = new HMACSHA256(key);
            return hash.ComputeHash(message);
        }

        private static byte[] HashSHA(byte[] innerKey, byte[] outerKey, byte[] message)
        {
            var hash = new SHA256Managed();

            // Compute the hash for the inner data first
            byte[] innerData = new byte[innerKey.Length + message.Length];
            Buffer.BlockCopy(innerKey, 0, innerData, 0, innerKey.Length);
            Buffer.BlockCopy(message, 0, innerData, innerKey.Length, message.Length);
            byte[] innerHash = hash.ComputeHash(innerData);

            // Compute the entire hash
            byte[] data = new byte[outerKey.Length + innerHash.Length];
            Buffer.BlockCopy(outerKey, 0, data, 0, outerKey.Length);
            Buffer.BlockCopy(innerHash, 0, data, outerKey.Length, innerHash.Length);
            byte[] result = hash.ComputeHash(data);

            return result;
        }

        private static byte[] StringEncode(string text)
        {
            var encoding = new ASCIIEncoding();
            return encoding.GetBytes(text);
        }

        private static string HashEncode(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private static byte[] HexDecode(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);
            }
            return bytes;
        }
        /// <summary>
        /// Hex hashing for HMAC
        /// </summary>
        /// <param name="keyHex">Key</param>
        /// <param name="message">message to hash</param>
        /// <returns></returns>
        private static string HashHMACHex(string keyHex, string message)
        {
            byte[] hash = HashHMAC(HexDecode(keyHex), StringEncode(message));
            return HashEncode(hash);
        }
        /// <summary>
        /// Hex hashing for SHA
        /// </summary>
        /// <param name="innerKeyHex">Inner key</param>
        /// <param name="outerKeyHex">Outer key</param>
        /// <param name="message">Message to hash</param>
        /// <returns></returns>
        private static string HashSHAHex(string innerKeyHex, string outerKeyHex, string message)
        {
            byte[] hash = HashSHA(HexDecode(innerKeyHex), HexDecode(outerKeyHex), StringEncode(message));
            return HashEncode(hash);
        }


        // Computes a keyed hash for a source file and creates a target file with the keyed hash
        // prepended to the contents of the source file.
        public static void SignFile(byte[] key, String sourceFile, String destFile)
        {
            // Initialize the keyed hash object.
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                using (FileStream inStream = new FileStream(sourceFile, FileMode.Open))
                {
                    using (FileStream outStream = new FileStream(destFile, FileMode.Create))
                    {
                        // Compute the hash of the input file.
                        byte[] hashValue = hmac.ComputeHash(inStream);
                        // Reset inStream to the beginning of the file.
                        inStream.Position = 0;
                        // Write the computed hash value to the output file.
                        outStream.Write(hashValue, 0, hashValue.Length);
                        // Copy the contents of the sourceFile to the destFile.
                        int bytesRead;
                        // read 1K at a time
                        byte[] buffer = new byte[1024];
                        do
                        {
                            // Read from the wrapping CryptoStream.
                            bytesRead = inStream.Read(buffer, 0, 1024);
                            outStream.Write(buffer, 0, bytesRead);
                        } while (bytesRead > 0);
                    }
                }
            }
            return;
        } // end SignFile

        // Compares the key in the source file with a new key created for the data portion of the file. If the keys
        // compare the data has not been tampered with.
        public static bool VerifyFile(byte[] key, String sourceFile)
        {
            bool err = false;
            // Initialize the keyed hash object.
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                // Create an array to hold the keyed hash value read from the file.
                byte[] storedHash = new byte[hmac.HashSize / 8];
                // Create a FileStream for the source file.
                using (FileStream inStream = new FileStream(sourceFile, FileMode.Open))
                {
                    // Read in the storedHash.
                    inStream.Read(storedHash, 0, storedHash.Length);
                    // Compute the hash of the remaining contents of the file.
                    // The stream is properly positioned at the beginning of the content,
                    // immediately after the stored hash value.
                    byte[] computedHash = hmac.ComputeHash(inStream);
                    // compare the computed hash with the stored value

                    for (int i = 0; i < storedHash.Length; i++)
                    {
                        if (computedHash[i] != storedHash[i])
                        {
                            err = true;
                        }
                    }
                }
            }
            if (err)
            {
                Console.WriteLine("Hash values differ! Signed file has been tampered with!");
                return false;
            }
            else
            {
                Console.WriteLine("Hash values agree -- no tampering occurred.");
                return true;
            }
        } //end VerifyFile

        public static void SignFileExample()
        {
            // Create a random key using a random number generator. This would be the
            //  secret key shared by sender and receiver.
            byte[] secretkey = new Byte[64];
            //RNGCryptoServiceProvider is an implementation of a random number generator.
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                // The array is now filled with cryptographically strong random bytes.
                rng.GetBytes(secretkey);

                // Use the secret key to sign the message file.
                //SignFile(secretkey, dataFile, signedFile);

                // Verify the signed file
                //VerifyFile(secretkey, signedFile);
            }
        }

    }




}
