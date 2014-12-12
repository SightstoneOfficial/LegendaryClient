#region

using System;
using System.IO;
using LegendaryClient.Logic.SQLite;

#endregion

namespace LegendaryClient.Logic.SoundLogic
{
    public class SelectChampion
    {
        public static int SelectChamp(Int32 champ)
        {
            Client.SoundPlayer.Source =
                new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "champions", champions.GetChampion(champ).name + ".mp3"));
            Client.SoundPlayer.Play();

            int hi = champ;

            return hi;
        }
    }
}