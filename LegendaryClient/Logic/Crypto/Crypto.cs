using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace LegendaryClient.Logic.Crypto
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
