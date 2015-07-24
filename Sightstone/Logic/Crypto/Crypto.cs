using System;
using System.Text;

namespace Sightstone.Logic.Crypto
{
    public class Crypto
    {
        public string EncodeString(string data)
        {
            return Encode(Encoding.Default.GetBytes(data));
        }
        public virtual string Encode(byte[] data)
        {
            throw new NotSupportedException("This algorithm does not support encrypting.");
        }

        public virtual byte[] Decode(string data)
        {
            throw new NotSupportedException("This algorithm does not support decrypting.");
        }
    }
}
