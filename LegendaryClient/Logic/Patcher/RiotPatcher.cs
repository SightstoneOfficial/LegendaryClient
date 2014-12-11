#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

#endregion

namespace LegendaryClient.Logic.Patcher
{
    public class RiotPatcher
    {
        public string DDragonVersion;

        public string GetDragon()
        {
            string dragonJson = "";
            using (var client = new WebClient())
            {
                try
                {
                    dragonJson = client.DownloadString("http://ddragon.leagueoflegends.com/realms/na.js");
                }
                catch (Exception)
                {
                }
            }
            dragonJson = dragonJson.Replace("Riot.DDragon.m=", "").Replace(";", "");
            var serializer = new JavaScriptSerializer();

            if (String.IsNullOrEmpty(dragonJson))
                return String.Empty;

            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(dragonJson);
            var version = (string) deserializedJson["v"];
            var cdn = (string) deserializedJson["cdn"];
            string s = cdn + "/dragontail-" + version + ".tgz";
            DDragonVersion = version;

            return s;
        }

        public string GetLatestAir()
        {
            string airVersions = "0.0.0";
            using (var client = new WebClient())
            {
                try
                {
                    airVersions =
                        client.DownloadString(
                            "http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/releaselisting_NA");
                }
                catch (WebException e)
                {
                    Client.Log(e.Message);
                }
            }
            return airVersions.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0];
        }

        public string GetLatestGame()
        {
            string gameVersions = "0.0.0";
            using (var client = new WebClient())
            {
                try
                {
                    gameVersions =
                        client.DownloadString(
                            "http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/releaselisting_NA");
                }
                catch (WebException e)
                {
                    Client.Log(e.Message);
                }
            }
            return gameVersions.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0];
        }

        public string GetCurrentAirInstall(string location)
        {
            var encoding = new ASCIIEncoding();
            var dInfo = new DirectoryInfo(location);
            DirectoryInfo[] subdirs;
            try
            {
                subdirs = dInfo.GetDirectories();
            }
            catch
            {
                return "0.0.0.0";
            }
            string latestVersion = "0.0.1";
            foreach (DirectoryInfo info in subdirs)
                latestVersion = info.Name;

            string airLocation = Path.Combine(location, latestVersion, "deploy");
            if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "ClientLibCommon.dat")))
            {
                File.Copy(Path.Combine(airLocation, "lib", "ClientLibCommon.dat"),
                    Path.Combine(Client.ExecutingDirectory, "ClientLibCommon.dat"));
            }
            if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "Client", "gameStats_en_US.sqlite")))
            {
                File.Copy(Path.Combine(airLocation, "assets", "data", "gameStats", "gameStats_en_US.sqlite"),
                    Path.Combine(Client.ExecutingDirectory, "Client", "gameStats_en_US.sqlite"), true);
            }
            else
            {
                File.Delete(Path.Combine(Client.ExecutingDirectory, "Client", "gameStats_en_US.sqlite"));
                File.Copy(Path.Combine(airLocation, "assets", "data", "gameStats", "gameStats_en_US.sqlite"),
                    Path.Combine(Client.ExecutingDirectory, "Client", "gameStats_en_US.sqlite"), true);
            }

            Copy(Path.Combine(airLocation, "assets", "images", "abilities"),
                Path.Combine(Client.ExecutingDirectory, "Assets", "abilities"));
            Copy(Path.Combine(airLocation, "assets", "images", "champions"),
                Path.Combine(Client.ExecutingDirectory, "Assets", "champions"));

            FileStream versionAir = File.Create(Path.Combine("Assets", "VERSION_AIR"));
            versionAir.Write(encoding.GetBytes(latestVersion), 0, encoding.GetBytes(latestVersion).Length);
            versionAir.Close();
            return latestVersion;
        }

        public string GetCurrentGameInstall(string gameLocation)
        {
            var encoding = new ASCIIEncoding();
            var dInfo = new DirectoryInfo(Path.Combine(gameLocation, "projects", "lol_game_client", "filearchives"));
            DirectoryInfo[] subdirs;
            try
            {
                subdirs = dInfo.GetDirectories();
            }
            catch
            {
                return "0.0.0.0";
            }
            string latestVersion = "0.0.1";
            foreach (DirectoryInfo info in subdirs)
                latestVersion = info.Name;

            var dInfox = new DirectoryInfo(Path.Combine(gameLocation, "solutions", "lol_game_client_sln", "releases"));
            DirectoryInfo[] subdirsx;
            try
            {
                subdirsx = dInfox.GetDirectories();
            }
            catch
            {
                return "0.0.0.0";
            }
            string latestVersionx = "0.0.1";
            foreach (DirectoryInfo info in subdirsx)
            {
                latestVersionx = info.Name;
            }
            //solutions\lol_game_client_sln\releases\0.0.1.48
            Copy(Path.Combine(gameLocation, "solutions", "lol_game_client_sln", "releases", latestVersionx, "deploy"),
                Path.Combine(Client.ExecutingDirectory, "RADS", "projects", "lol_game_client_ko_kr"));


            if (Directory.Exists(Path.Combine(gameLocation, "projects", "lol_game_client_ko_kr")))
            {
                Copy(Path.Combine(gameLocation, "projects", "lol_game_client_ko_kr"),
                    Path.Combine(Client.ExecutingDirectory, "RADS", "projects", "lol_game_client_ko_kr"));
            }
            else if (Directory.Exists(Path.Combine(gameLocation, "projects", "lol_game_client_en_gb")))
            {
                Copy(Path.Combine(gameLocation, "projects", "lol_game_client_en_gb"),
                    Path.Combine(Client.ExecutingDirectory, "RADS", "projects", "lol_game_client_en_gb"));
            }
            else if (Directory.Exists(Path.Combine(gameLocation, "projects", "lol_game_client_en_us")))
            {
                Copy(Path.Combine(gameLocation, "projects", "lol_game_client_en_us"),
                    Path.Combine(Client.ExecutingDirectory, "RADS", "projects", "lol_game_client_en_us"));
            }

            string parentDirectory = Directory.GetParent(gameLocation).FullName;
            Copy(Path.Combine(parentDirectory, "Config"), Path.Combine(Client.ExecutingDirectory, "Config"));

            Copy(Path.Combine(gameLocation, "solutions", "lol_game_client_sln"),
                Path.Combine(Client.ExecutingDirectory, "RADS", "projects", "lol_game_client"));

            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "RADS", "RiotRadsIO.dll")))
                File.Delete(Path.Combine(Client.ExecutingDirectory, "RADS", "RiotRadsIO.dll"));

            File.Copy(Path.Combine(gameLocation, "RiotRadsIO.dll"),
                Path.Combine(Client.ExecutingDirectory, "RADS", "RiotRadsIO.dll"));

            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL")))
                File.Delete(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL"));

            FileStream versionAir = File.Create(Path.Combine("RADS", "VERSION_LOL"));
            versionAir.Write(encoding.GetBytes(latestVersion), 0, encoding.GetBytes(latestVersion).Length);
            //VersionAIR.Write(encoding.GetBytes(GetLatestGame()), 0, encoding.GetBytes(GetLatestGame()).Length);
            versionAir.Close();

            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSIONGC_LOL")))
                File.Delete(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSIONGC_LOL"));

            FileStream versionGc = File.Create(Path.Combine("RADS", "VERSIONGC_LOL"));
            versionGc.Write(encoding.GetBytes(latestVersionx), 0, encoding.GetBytes(latestVersionx).Length);
            versionGc.Close();

            //return latestVersionx;
            return latestVersion;
        }

        private static void Copy(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir).Where(file => file != null))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);

            foreach (string directory in Directory.GetDirectories(sourceDir).Where(file => file != null))
                Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
        }
    }
}