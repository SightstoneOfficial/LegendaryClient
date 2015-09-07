// ReSharper disable InconsistentNaming
#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms.VisualStyles;
using ComponentAce.Compression.Libs.zlib;
using Sightstone.Patcher.Logic.Region;

#endregion

namespace Sightstone.Patcher.Logic
{
    internal class LeagueDownloadLogic
    {
        public static string ReleaseListing = "";
        public static string SolutionManifest = "";


        /// <summary>
        ///     Gets the Latest LeagueOfLegends lol_game_client_sln version
        /// </summary>
        public static string[] GetLolClientClientVersion(MainRegion Region)
        {
            //Get the GameClientSln version
            using (var client = new WebClient())
            {
                ReleaseListing = client.DownloadString(Region.GameReleaseListingUri);
            }

            return ReleaseListing.Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToArray();
        }

        public static string[] GetLolClientSlnVersion(MainRegion Region)
        {
            using (var client = new WebClient())
            {
                ReleaseListing = client.DownloadString(Region.GameSlnReleaseListingUri);
            }

            return ReleaseListing.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToArray();
        }


        public static string[] GetLolClientVersion(MainRegion Region)
        {
            //Get the GameClientSln version
            using (var client = new WebClient())
            {
                ReleaseListing = client.DownloadString(Region.ReleaseListingUri);
            }

            return ReleaseListing.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToArray();
        }

        public static string GetLatestLCLOLVersion()
        {
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LC_LOL.Version")))
            {
                var version = File.ReadAllLines(Path.Combine(Client.ExecutingDirectory, "LC_LOL.Version"));
                if (version.Count() > 0)
                    return version[0];
            }
            var encoding = new ASCIIEncoding();
            File.Create(Path.Combine(Client.ExecutingDirectory, "LC_LOL.Version")).Write(encoding.GetBytes("0.0.0.0"), 0, encoding.GetBytes("0.0.0.0").Length);
            return "0.0.0.0";
        }

        public static string GetLatestLCClientVersion()
        {
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LC_LOLCLIENT.Version")))
            {
                var version = File.ReadAllLines(Path.Combine(Client.ExecutingDirectory, "LC_LOLCLIENT.Version"));
                if (version.Count() > 0)
                    return version[0];
            }
            var encoding = new ASCIIEncoding();
            File.Create(Path.Combine(Client.ExecutingDirectory, "LC_LOLCLIENT.Version")).Write(encoding.GetBytes("0.0.0.0"), 0, encoding.GetBytes("0.0.0.0").Length);
            return "0.0.0.0";
        }

        /// <summary>
        ///     Gets the SolutionManifest
        /// </summary>
        /// <returns>
        ///     The SolutionManifest file from riot
        /// </returns>
        public static string GetConfigurationManifest(MainRegion Region)
        {
            string latestSlnVersion = GetLolClientClientVersion(Region)[0];
            //Get GameClient Language files
            using (var client = new WebClient())
            {
                SolutionManifest =
                    client.DownloadString(
                        "http://l3cdn.riotgames.com/releases/live/solutions/lol_game_client_sln/releases/" +
                        latestSlnVersion + "/solutionmanifest");
            }
            return SolutionManifest;
        }

        public static Uri[] GetUris(MainRegion Region)
        {
            string[] packagemanifest;
            var versions = GetLolClientClientVersion(Region);
            var localVersion = GetLatestLCLOLVersion();
            var notInstalled = versions.TakeWhile(version => version != localVersion).ToList();
            using (var client = new WebClient())
            {
                packagemanifest = client.DownloadString(Region.GameClientUpdateUri).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
            }
            var resultUris = (from uris in packagemanifest from toInstall in notInstalled where uris.Contains(toInstall) select new Uri("http://l3cdn.riotgames.com/releases/live" + uris.Split(',')[0]));
            return resultUris.ToArray();
        }

        public static Uri[] ClientGetUris(MainRegion Region)
        {
            string[] packagemanifest;
            var versions = GetLolClientVersion(Region);
            var localVersion = GetLatestLCClientVersion();
            var notInstalled = versions.TakeWhile(version => version != localVersion).ToList();
            using (var client = new WebClient())
            {
                packagemanifest = client.DownloadString(Region.ClientUpdateUri).Split(new[] { Environment.NewLine }, StringSplitOptions.None).Skip(1).ToArray();
            }
            var resultUris = (from uris in packagemanifest from toInstall in notInstalled where uris.Contains(toInstall) select new Uri("http://l3cdn.riotgames.com/releases/live" + uris.Split(',')[0]));
            return resultUris.ToArray();
        }

        public static void DecompressFile(string inFile, string outFile)
        {
            int data;
            const int stopByte = -1;
            var outFileStream = new FileStream(outFile, FileMode.Create);
            var inZStream = new ZInputStream(File.Open(inFile, FileMode.Open, FileAccess.Read));

            while (stopByte != (data = inZStream.Read()))
            {
                var databyte = (byte) data;
                outFileStream.WriteByte(databyte);
            }

            inZStream.Close();
            outFileStream.Close();
        }
    }
}