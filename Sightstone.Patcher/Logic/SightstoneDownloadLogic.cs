using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Sightstone.Patcher.Logic
{
    public class SightstoneDownloadLogic
    {
        internal static string GetAppVeyorDlUrl()
        {
            string jobId = string.Empty;
            using (WebClient client = new WebClient())
            {
                var x = client.DownloadString("https://ci.appveyor.com/api/projects/EddyV/Sightstone/");
                Appveyor json = JsonConvert.DeserializeObject<Appveyor>(x);
                jobId = json.build.jobs[0].jobId;
                return "https://ci.appveyor.com/api/buildjobs/" + jobId + "/artifacts/Sightstone.zip";
            }
        }
    }
}
