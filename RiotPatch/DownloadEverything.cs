using RiotPatch.RADS.projects.lol_air_client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiotPatch
{
    public class DownloadEverything
    {
        public static void DownloadEverything(string MainLocation)
        {
            //Create needed folders because they do not exist
            if (!Directory.Exists(Path.Combine(MainLocation, "RADS")))
            {
                Directory.CreateDirectory(Path.Combine(MainLocation, "RADS"));

                Directory.CreateDirectory(Path.Combine(MainLocation, "RADS", "projects"));
                Directory.CreateDirectory(Path.Combine(MainLocation, "RADS", "projects", "lol_air_client"));
                Directory.CreateDirectory(Path.Combine(MainLocation, "RADS", "projects", "lol_game_client"));
                Directory.CreateDirectory(Path.Combine(MainLocation, "RADS", "projects", "lol_launcher"));
                Directory.CreateDirectory(Path.Combine(MainLocation, "RADS", "projects", "lol_patcher"));

                Directory.CreateDirectory(Path.Combine(MainLocation, "RADS", "solutions"));
                Directory.CreateDirectory(Path.Combine(MainLocation, "RADS", "solutions", "lol_game_client_sln"));

                Directory.CreateDirectory(Path.Combine(MainLocation, "RADS", "system"));
            }
            DownloadAir.EasyDownloader(Path.Combine(MainLocation, "RADS"));
        }
    }
}
