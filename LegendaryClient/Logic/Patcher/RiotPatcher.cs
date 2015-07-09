using LegendaryClient.Logic.MultiUser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

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

            if (string.IsNullOrEmpty(dragonJson))
                return string.Empty;

            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(dragonJson);
            var version = (string) deserializedJson["v"];
            var cdn = (string) deserializedJson["cdn"];
            string s = cdn + "/dragontail-" + version + ".tgz";
            DDragonVersion = version;

            return s;
        }

        public string GetListing(string listingLink)
        {
            string versions = "0.0.0";
            using (var client = new WebClient())
            {
                try
                {
                    versions =
                        client.DownloadString(
                             listingLink);
                }
                catch (WebException e)
                {
                    Client.Log(e.Message);
                }
            }
            return versions.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0];
        }

        public string[] GetManifest(string manifestLink)
        {   
            string Manifest = null;
            string[] Manifest2 = null;
            using (var client = new WebClient())
            {
                
                try
                {
                    Manifest = 
                        client.DownloadString(
                             manifestLink);
                    Manifest2 = Manifest.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                }
                catch (WebException e)
                {
                    Client.Log(e.Message);
                }
            }
            return Manifest2;
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