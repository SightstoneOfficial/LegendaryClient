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
        [Obsolete("List is obsolete please use getDevelopers() instead")]
         public static List<string> Developers = new string[] { "700514b8ce9072adc83ed189af0820edfd1731ac", "408bca02f44877bdeaef25d685544b6dcb66ab6c" }.OfType<string>().ToList(); //add your sha1 hashed summoner info. Format is Summoner Region

        public static List<string> getDevelopers()
        {
            List<string> returnList = new List<string>();
            WebClient client = new WebClient();
            Stream stream = client.OpenRead("http://legendaryclient.net/devs.txt");
            StreamReader reader = new StreamReader(stream);
            String content = reader.ReadToEnd();
            string[] x = content.Split('\n');
            foreach (string Element in x)
            {
                returnList.Add(Element);
            }
            return returnList;
        }
        
    }
}
