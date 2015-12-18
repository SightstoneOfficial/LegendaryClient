using System.Text;

namespace Sightstone.Core.Helpers.Converters
{
    /// <summary>
    /// Converts strings into bytes
    /// </summary>
    public static class ByteHelper
    {
        /// <summary>
        /// Converts a string to a byte array
        /// </summary>
        /// <param name="input">the input string</param>
        /// <returns>the encoded bytes</returns>
        public static byte[] ToByte(this string input)
        {
            return Encoding.Default.GetBytes(input);
        }

        /// <summary>
        /// Converts a string to a byte array
        /// </summary>
        /// <param name="input">the input string</param>
        /// <param name="encoding">the encoding type <seealso cref="Encoding.ASCII"/> <seealso cref="Encoding.Default"/></param>
        /// <returns>the encoded bytes</returns>
        public static byte[] ToByte(this string input, Encoding encoding)
        {
            return encoding.GetBytes(input);
        }
    }
}
