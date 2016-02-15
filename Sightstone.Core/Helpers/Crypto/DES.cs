using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Sightstone.Core.Helpers.Converters;

namespace Sightstone.Core.Helpers.Crypto
{
    /// <summary>
    /// DES Crypto Helper
    /// </summary>
    public static class DES
    {
        public static string Encrypt(string input, string pass)
        {
            var hash = EncryptStringToBytes(input, SHA1.Encrypt(pass.ToByte()), MD5.Encrypt(pass.ToByte()));
            var sb = new StringBuilder(hash.Length * 2);

            foreach (var b in hash)
            {
                sb.Append(b.ToString());
            }

            return sb.ToString();
        }

        public static string ToDES(this string input, string pass)
        {
            return Encrypt(input, pass);
        }

        public static string DecryptDES(this string input, string pass)
        {
            return Decrypt(input, pass);
        }

        public static string Decrypt(string input, string pass)
        {
            return DecryptStringFromBytes(input.ToByte(), SHA1.Encrypt(pass.ToByte()), MD5.Encrypt(pass.ToByte()));
        }


        private static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;
            using (var tdsAlg = new DESCryptoServiceProvider())
            {
                tdsAlg.Key = Key;
                tdsAlg.IV = IV;
                
                ICryptoTransform encryptor = tdsAlg.CreateEncryptor(tdsAlg.Key, tdsAlg.IV);
                
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            
            string plaintext = null;
            
            using (var tdsAlg = new DESCryptoServiceProvider())
            {
                tdsAlg.Key = Key;
                tdsAlg.IV = IV;
                
                var decryptor = tdsAlg.CreateDecryptor(tdsAlg.Key, tdsAlg.IV);
                
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }
            return plaintext;
        }
    }
}
