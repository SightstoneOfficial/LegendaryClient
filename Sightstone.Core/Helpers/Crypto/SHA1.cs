using System.Security.Cryptography;
using System.Text;

namespace Sightstone.Core.Helpers.Crypto
{
    /// <summary>
    /// Methods to encrypt strings or bytes using SHA1
    /// </summary>
    public static class SHA1
    {
        public static string Encrypt(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (var b in hash)
                {
                    sb.Append(b.ToString());
                }

                return sb.ToString();
            }
        }

        public static string ToSHA1(this string input)
        {
            return Encrypt(input);
        }

        public static byte[] Encrypt(byte[] input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                return sha1.ComputeHash(input);
            }
        }

        public static byte[] ToSHA1(this byte[] input)
        {
            return Encrypt(input);
        }
    }
}
