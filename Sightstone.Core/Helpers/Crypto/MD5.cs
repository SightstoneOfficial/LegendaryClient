using System.Security.Cryptography;
using System.Text;

namespace Sightstone.Core.Helpers.Crypto
{
    public static class MD5
    {
        public static string Encrypt(string input)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (var b in hash)
                {
                    sb.Append(b.ToString());
                }

                return sb.ToString();
            }
        }

        public static string ToMD5(this string input)
        {
            return Encrypt(input);
        }

        public static byte[] Encrypt(byte[] input)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                return md5.ComputeHash(input);
            }
        }

        public static byte[] ToMD5(this byte[] input)
        {
            return Encrypt(input);
        }
    }
}
