using LegendaryClient.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.SoundLogic
{
    public class SelectChampion
    {
        public static int SelectChamp(Int32 Champ)
        {
            Client.SoundPlayer.Source = new Uri(Path.Combine(Client.ExecutingDirectory, "Champions", champions.GetChampion(Champ).name + ".mp3"));
            Client.SoundPlayer.Play();
            int Hi = Champ;
            return Hi;
        }
    }
}
