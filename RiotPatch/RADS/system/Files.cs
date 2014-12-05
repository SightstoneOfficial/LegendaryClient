#region

using System;
using System.IO;

#endregion

namespace RiotPatch.RADS.system
{
    public class Files
    {
        public static void EasyDownloader(string mainDownloadLocation, bool disableP2P = true, string locale = "ko_KR",
            string region = "na", string airproject = "lol_air_client", string gameProject = "lol_game_client_sln")
        {
            FileStream file = File.Create(Path.Combine(mainDownloadLocation, "system", "launcher.cfg"));
            file.Close();
            file.Dispose();
            var writer = new StreamWriter(Path.Combine(mainDownloadLocation, "system", "launcher.cfg"));
            writer.Write(
                "airConfigProject = lol_air_client_config_{0} {1} airProject = {2} {1} gameProject = {3} {1} installation_id = {4}",
                region, Environment.NewLine, airproject, gameProject, "N159hA==");
            writer.Close();

            File.Create(Path.Combine(mainDownloadLocation, "system", "locale.cfg"));
            writer = new StreamWriter(Path.Combine(mainDownloadLocation, "system", "locale.cfg"));
            writer.Write("locale = " + locale);
            writer.Close();

            File.Create(Path.Combine(mainDownloadLocation, "system", "system.cfg"));
            writer = new StreamWriter(Path.Combine(mainDownloadLocation, "system", "system.cfg"));
            writer.Write("DownloadPath = /releases/live" + Environment.NewLine + "DownloadURL = l3cdn.riotgames.com" +
                         Environment.NewLine + "Region = " + region.ToUpper());
            writer.Close();

            File.Create(Path.Combine(mainDownloadLocation, "system", "user.cfg"));
            writer = new StreamWriter(Path.Combine(mainDownloadLocation, "system", "user.cfg"));
            writer.Write("disableP2P = " + disableP2P);
            writer.Close();
        }
    }
}