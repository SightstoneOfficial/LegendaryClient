using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RtmpSharp.IO.AMF3;

namespace RtmpSharp.IO
{
    public static class Uuid
    {
        private static readonly Random random = new Random();

        public static string NewUuid()
        {
            return Guid.NewGuid().ToString("D").ToUpperInvariant();
        }

        public static string ToString(ByteArray b)
        {
            if (b == null || b.Length != 16) return null;
            byte[] arr = b.ToArray();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < arr.Length; i++)
            {

                if (i == 4 || i == 6 || i == 8 || i == 10)
                {
                    sb.Append('-');
                }
                sb.AppendFormat("{0:X2}", arr[i]);
            }
            return sb.ToString();
        }

        public static ByteArray ToBytes(string s)
        {
            if (s == null || s.Length != 36)
                return null;

            s = s.Replace("-", "");

            var ret = new ByteArray();
            for (int i = 0; i < s.Length; i += 2)
            {
                byte num;
                if (!byte.TryParse(s.Substring(i, 2), NumberStyles.HexNumber, null, out num))
                    return null;
                ret.WriteByte(num);
            }
            ret.Position = 0;
            return ret;
        }
    }
}
