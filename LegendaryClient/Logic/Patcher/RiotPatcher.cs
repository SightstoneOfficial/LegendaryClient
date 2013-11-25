using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace LegendaryClient.Logic.Patcher
{
    public class RiotPatcher
    {
        public RiotPatcher()
        {
            
        }

        public string GetDragon()
        {
            string spectatorJSON = "";
            using (WebClient client = new WebClient())
            {
                spectatorJSON = client.DownloadString("http://ddragon.leagueoflegends.com/realms/na.js");
            }
            spectatorJSON = spectatorJSON.Replace("Riot.DDragon.m=", "").Replace(";", "");
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(spectatorJSON);
            string Version = (string)deserializedJSON["v"];
            string CDN = (string)deserializedJSON["cdn"];
            string s = CDN + "/dragontail-" + Version + ".tgz";
            return s;
        }
    }
}
