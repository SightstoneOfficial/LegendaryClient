using Sightstone.Logic.MultiUser;
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
                using (WebClient client = new WebClient())
                {
                    var content = client.DownloadString("http://legendaryclient.net/devs.txt");
                    var devs = content.Split('\n').ToList();
                    devs.RemoveAll(x => x.StartsWith("#"));
                    return devs;
                }
            }
            catch
            {
                return new List<string>();
            }
        }
        
    }
}
