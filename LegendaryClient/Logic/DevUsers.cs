using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic
{
    class DevUsers
    {
        private static string[] Developers = { "34e006b49f60bbb25d15e8ee937fdf5a04a4f6c5" }; //add your sha1 hashed usernames here
        public static string[] getDevs()
        {
            return Developers;
        }
    }
}
