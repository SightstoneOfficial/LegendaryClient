using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.SoundLogic
{
    public class AmbientChampSelect
    {
        public static void PlayAmbientChampSelectSound(ChampSelectSound SelectedSound)
        {
            Client.AmbientSoundPlayer.Source = new Uri(Path.Combine(Client.ExecutingDirectory, "ambient", SelectedSound + ".mp3"));
        }
    }
}
