using System;
using System.Text;

namespace LegendaryClient.Logic.Crypto
{
    public class Crypto
    {
        public string Hash(string data)
        {
            return Hash(Encoding.Default.GetBytes(data));
        }
        public virtual string Hash(byte[] data)
        {
            throw new NotSupportedException("This algorithm does not support encrypting.");
        }

        public virtual byte[] Decode(string data)
        {
            throw new NotSupportedException("This algorithm does not support decrypting.");
        }
    }
}
