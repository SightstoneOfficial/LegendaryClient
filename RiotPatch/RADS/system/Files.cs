using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiotPatch.RADS.system
{
    public class Files
    {
        public static void EasyDownloader(string MainDownloadLocation, bool disableP2P = true, string Locale = "ko_KR", string region = "na", string Airproject = "lol_air_client", string GameProject = "lol_game_client_sln")
        {
            var file = File.Create(Path.Combine(MainDownloadLocation, "system", "launcher.cfg"));
            file.Close();
            file.Dispose();
            StreamWriter writer = new StreamWriter(Path.Combine(MainDownloadLocation, "system", "launcher.cfg"));
            writer.Write(string.Format(
                "airConfigProject = lol_air_client_config_{0} {1} airProject = {2} {1} gameProject = {3} {1} installation_id = {4}", 
                region, Environment.NewLine, Airproject, GameProject, "N159hA=="));
            writer.Close();

            file = File.Create(Path.Combine(MainDownloadLocation, "system", "locale.cfg"));
            writer = new StreamWriter(Path.Combine(MainDownloadLocation, "system", "locale.cfg"));
            writer.Write("locale = " + Locale);
            writer.Close();

            file = File.Create(Path.Combine(MainDownloadLocation, "system", "system.cfg"));
            writer = new StreamWriter(Path.Combine(MainDownloadLocation, "system", "system.cfg"));
            writer.Write("DownloadPath = /releases/live" + Environment.NewLine+ "DownloadURL = l3cdn.riotgames.com" + Environment.NewLine + "Region = " + region.ToUpper());
            writer.Close();

            file = File.Create(Path.Combine(MainDownloadLocation, "system", "user.cfg"));
            writer = new StreamWriter(Path.Combine(MainDownloadLocation, "system", "user.cfg"));
            writer.Write("disableP2P = " + disableP2P);
            writer.Close();
        }
    }
}
