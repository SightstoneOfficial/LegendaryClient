using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sightstone.Logic
{
    class DevUsers
    {
        public static List<string> getDevelopers()
        {
            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead("http://Sightstone.net/devs.txt");
                StreamReader reader = new StreamReader(stream);
                string content = reader.ReadToEnd();
                var devs = content.Split('\n').ToList();
                devs.RemoveAll(x => x.StartsWith("#"));
                return devs;
            }
            catch
            {
                return new List<string>();
            }
        }
        
    }
}
