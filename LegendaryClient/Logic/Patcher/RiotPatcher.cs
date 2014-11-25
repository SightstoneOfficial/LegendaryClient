using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace LegendaryClient.Logic.Patcher
{
    public class RiotPatcher
    {
        public string DDragonVersion;

        public RiotPatcher()
        {
        }

        public string GetDragon()
        {
            string dragonJSON = "";
            using (WebClient client = new WebClient())
            {
                try
                {
                    dragonJSON = client.DownloadString("http://ddragon.leagueoflegends.com/realms/na.js");
                }
                catch
                {
                    
                }
            }
            dragonJSON = dragonJSON.Replace("Riot.DDragon.m=", "").Replace(";", "");
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (!String.IsNullOrEmpty(dragonJSON))
            {
                Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(dragonJSON);
                string Version = (string)deserializedJSON["v"];
                string CDN = (string)deserializedJSON["cdn"];
                string s = CDN + "/dragontail-" + Version + ".tgz";
                DDragonVersion = Version;
                return s;
            }
            else return String.Empty;
        }

        public string GetLatestAir()
        {
            string airVersions = "";
            using (WebClient client = new WebClient())
            {
                airVersions = client.DownloadString("http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/releaselisting_NA");
            }
            return airVersions.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
        }

        public string GetLatestGame()
        {
            string gameVersions = "";
            using (WebClient client = new WebClient())
            {
                gameVersions = client.DownloadString("http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/releaselisting_NA");
            }
            return gameVersions.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
        }

        public string GetCurrentAirInstall(string Location)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            DirectoryInfo dInfo = new DirectoryInfo(Location);
            DirectoryInfo[] subdirs = null;
            try
            {
                subdirs = dInfo.GetDirectories();
            }
            catch { return "0.0.0.0"; }
            string latestVersion = "0.0.1";
            foreach (DirectoryInfo info in subdirs)
            {
                latestVersion = info.Name;
            }

            string AirLocation = Path.Combine(Location, latestVersion, "deploy");
            if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "ClientLibCommon.dat")))
            {
                File.Copy(Path.Combine(AirLocation, "lib", "ClientLibCommon.dat"), Path.Combine(Client.ExecutingDirectory, "ClientLibCommon.dat"));
            }
            if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "Client", "gameStats_en_US.sqlite")))
            {
                File.Copy(Path.Combine(AirLocation, "assets", "data", "gameStats", "gameStats_en_US.sqlite"), Path.Combine(Client.ExecutingDirectory, "Client", "gameStats_en_US.sqlite"));
            }
            else
            {
                File.Delete(Path.Combine(Client.ExecutingDirectory, "Client", "gameStats_en_US.sqlite"));
                File.Copy(Path.Combine(AirLocation, "assets", "data", "gameStats", "gameStats_en_US.sqlite"), Path.Combine(Client.ExecutingDirectory, "Client", "gameStats_en_US.sqlite"));
            }

            Copy(Path.Combine(AirLocation, "assets", "images", "abilities"), Path.Combine(Client.ExecutingDirectory, "Assets", "abilities"));
            Copy(Path.Combine(AirLocation, "assets", "images", "champions"), Path.Combine(Client.ExecutingDirectory, "Assets", "champions"));

            var VersionAIR = File.Create(Path.Combine("Assets", "VERSION_AIR"));
            VersionAIR.Write(encoding.GetBytes(latestVersion), 0, encoding.GetBytes(latestVersion).Length);
            VersionAIR.Close();
            return latestVersion;
        }

        public string GetCurrentGameInstall(string GameLocation)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            DirectoryInfo dInfo = new DirectoryInfo(Path.Combine(GameLocation, "projects", "lol_game_client", "filearchives"));
            DirectoryInfo[] subdirs = null;
            try
            {
                subdirs = dInfo.GetDirectories();
            }
            catch { return "0.0.0.0"; }
            string latestVersion = "0.0.1";
            foreach (DirectoryInfo info in subdirs)
            {
                latestVersion = info.Name;
            }

            DirectoryInfo dInfox = new DirectoryInfo(Path.Combine(GameLocation, "solutions", "lol_game_client_sln", "releases"));
            DirectoryInfo[] subdirsx = null;
            try
            {
                subdirsx = dInfox.GetDirectories();
            }
            catch { return "0.0.0.0"; }
            string latestVersionx = "0.0.1";
            foreach (DirectoryInfo info in subdirsx)
            {
                latestVersionx = info.Name;
            }
            //solutions\lol_game_client_sln\releases\0.0.1.48
            Copy(Path.Combine(GameLocation, "solutions", "lol_game_client_sln", "releases", latestVersionx, "deploy"), Path.Combine(Client.ExecutingDirectory, "RADS", "projects", "lol_game_client_ko_kr"));

            
            if (Directory.Exists(Path.Combine(GameLocation, "projects", "lol_game_client_ko_kr")))
            {
                Copy(Path.Combine(GameLocation, "projects", "lol_game_client_ko_kr"), Path.Combine(Client.ExecutingDirectory, "RADS", "projects", "lol_game_client_ko_kr"));
            }
            else if (Directory.Exists(Path.Combine(GameLocation, "projects", "lol_game_client_en_gb")))
            {
                Copy(Path.Combine(GameLocation, "projects", "lol_game_client_en_gb"), Path.Combine(Client.ExecutingDirectory, "RADS", "projects", "lol_game_client_en_gb"));
            }
            else if (Directory.Exists(Path.Combine(GameLocation, "projects", "lol_game_client_en_us")))
            {
                Copy(Path.Combine(GameLocation, "projects", "lol_game_client_en_us"), Path.Combine(Client.ExecutingDirectory, "RADS", "projects", "lol_game_client_en_us"));
            }

            string ParentDirectory = Directory.GetParent(GameLocation).FullName;
            Copy(Path.Combine(ParentDirectory, "Config"), Path.Combine(Client.ExecutingDirectory, "Config"));

            Copy(Path.Combine(GameLocation, "solutions", "lol_game_client_sln"), Path.Combine(Client.ExecutingDirectory, "RADS", "projects", "lol_game_client"));

            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "RADS", "RiotRadsIO.dll")))
            {
                File.Delete(Path.Combine(Client.ExecutingDirectory, "RADS", "RiotRadsIO.dll"));
            }
            File.Copy(Path.Combine(GameLocation, "RiotRadsIO.dll"), Path.Combine(Client.ExecutingDirectory, "RADS", "RiotRadsIO.dll"));

            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL")))
            {
                File.Delete(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL"));
            }
            var VersionAIR = File.Create(Path.Combine("RADS", "VERSION_LOL"));
            VersionAIR.Write(encoding.GetBytes(latestVersion), 0, encoding.GetBytes(latestVersion).Length);
            //VersionAIR.Write(encoding.GetBytes(GetLatestGame()), 0, encoding.GetBytes(GetLatestGame()).Length);
            VersionAIR.Close();

            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSIONGC_LOL")))
            {
                File.Delete(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSIONGC_LOL"));
            }
            var VersionGC = File.Create(Path.Combine("RADS", "VERSIONGC_LOL"));
            VersionGC.Write(encoding.GetBytes(latestVersionx), 0, encoding.GetBytes(latestVersionx).Length);
            VersionGC.Close();

            //return latestVersionx;
            return latestVersion;
        }

        private void Copy(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);

            foreach (var directory in Directory.GetDirectories(sourceDir))
                Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
        }
    }
}