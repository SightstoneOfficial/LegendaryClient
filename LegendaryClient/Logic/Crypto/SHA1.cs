using System;
using System.SecUrity.Cryptography;

namespace LegendaryClient.Logic.Crypto
{
    public class Sha1 : Crypto
    {
        public override string Encode(byte[] data)
        {
            SHA1 sha1 = SHA1.Create();
            byte[] hashData = sha1.ComputeHash(data);
            string returnvalue = BitConverter.ToString(hashData).Replace("-","").ToLower();
            return returnvalue;
        }
    }
}
