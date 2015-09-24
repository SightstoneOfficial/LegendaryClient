using Sightstone.Logic.MultiUser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace Sightstone.Logic.Riot
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
                dragonJSON = client.DownloadString("http://ddragon.leagueoflegends.com/realms/na.js");
            }
            dragonJSON = dragonJSON.Replace("Riot.DDragon.m=", "").Replace(";", "");
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(dragonJSON);
            string Version = (string)deserializedJSON["v"];
            string CDN = (string)deserializedJSON["cdn"];
            string s = CDN + "/dragontail-" + Version + ".tgz";
            DDragonVersion = Version;
            return s;
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
            if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "gameStats_en_US.sqlite")))
            {
                File.Copy(Path.Combine(AirLocation, "assets", "data", "gameStats", "gameStats_en_US.sqlite"), Path.Combine(Client.ExecutingDirectory, "gameStats_en_US.sqlite"));
            }

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

            string ParentDirectory = Directory.GetParent(GameLocation).FullName;
            if (Directory.Exists(Path.Combine(ParentDirectory, "Config")))
            {
                Copy(Path.Combine(ParentDirectory, "Config"), Path.Combine(Client.ExecutingDirectory, "Config"));
            }

            Copy(Path.Combine(GameLocation, "projects", "lol_game_client"), Path.Combine(Client.ExecutingDirectory, "RADS", "projects", "lol_game_client"));
            File.Copy(Path.Combine(GameLocation, "RiotRadsIO.dll"), Path.Combine(Client.ExecutingDirectory, "RADS", "RiotRadsIO.dll"));

            var VersionAIR = File.Create(Path.Combine("RADS", "VERSION_LOL"));
            VersionAIR.Write(encoding.GetBytes(latestVersion), 0, encoding.GetBytes(latestVersion).Length);
            VersionAIR.Close();
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