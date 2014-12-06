#region

using System.IO;
using RiotPatch.RADS.projects.lol_air_client;

#endregion

namespace RiotPatch
{
    public class DownloadEverything
    {
        public static void DownloadEverythingFix(string mainLocation)
        {
            //Create needed folders because they do not exist
            if (!Directory.Exists(Path.Combine(mainLocation, "RADS")))
            {
                Directory.CreateDirectory(Path.Combine(mainLocation, "RADS"));

                Directory.CreateDirectory(Path.Combine(mainLocation, "RADS", "projects"));
                Directory.CreateDirectory(Path.Combine(mainLocation, "RADS", "projects", "lol_air_client"));
                Directory.CreateDirectory(Path.Combine(mainLocation, "RADS", "projects", "lol_game_client"));
                Directory.CreateDirectory(Path.Combine(mainLocation, "RADS", "projects", "lol_launcher"));
                Directory.CreateDirectory(Path.Combine(mainLocation, "RADS", "projects", "lol_patcher"));

                Directory.CreateDirectory(Path.Combine(mainLocation, "RADS", "solutions"));
                Directory.CreateDirectory(Path.Combine(mainLocation, "RADS", "solutions", "lol_game_client_sln"));

                Directory.CreateDirectory(Path.Combine(mainLocation, "RADS", "system"));
            }
            DownloadAir.EasyDownloader(Path.Combine(mainLocation, "RADS"));
        }
    }
}