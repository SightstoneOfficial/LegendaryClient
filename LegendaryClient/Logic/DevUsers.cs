using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic
{
    class DevUsers
    {
        public static List<string> getDevelopers()
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead("http://legendaryclient.net/devs.txt");
            StreamReader reader = new StreamReader(stream);
            String content = reader.ReadToEnd();
            return content.Split('\n').ToList();
        }
        
    }
}
