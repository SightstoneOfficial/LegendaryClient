using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic
{
    class DevUsers
    {
        public static List<string> Developers = new string[] { "700514b8ce9072adc83ed189af0820edfd1731ac", "408bca02f44877bdeaef25d685544b6dcb66ab6c" }.OfType<string>().ToList(); //add your sha1 hashed summoner info. Format is Summoner Region
    }
}
