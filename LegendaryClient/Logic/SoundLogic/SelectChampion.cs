using System;
using System.IO;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Logic.MultiUser;

namespace LegendaryClient.Logic.SoundLogic
{
    public class SelectChampion
    {
        public static int SelectChamp(int champ)
        {
            Client.SoundPlayer.Source =
                new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "champions", champions.GetChampion(champ).name + ".mp3"));
            Client.SoundPlayer.Play();


            int hi = champ;

            return hi;
        }
    }
}