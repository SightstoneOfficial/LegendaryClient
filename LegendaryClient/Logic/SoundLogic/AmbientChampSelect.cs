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
        public static string currentQueueToSoundFile(double QueueId )
        {
            string SoundFileName = "";
            if (QueueId == 2)
            {
                SoundFileName = "ChmpSlct_BlindPick";
            }
            else if (QueueId == 4)
            {
                SoundFileName = "ChmpSlct_DraftMode";
            }
            else if (QueueId == 61)
            {
                SoundFileName = "ChmpSlct_DraftMode";
            }
            else if (QueueId == 31)
            {
                SoundFileName = "ChmpSlct_BlindPick";
            }
            else if (QueueId == 32)
            {
                SoundFileName = "ChmpSlct_BlindPick";
            }
            else if (QueueId == 42)
            {
                SoundFileName = "ChmpSlct_DraftMode";
            }
            else if (QueueId == 65)
            {
                SoundFileName = "howlingabyss_champselect";
            }
            else if (QueueId == 32)
            {
                SoundFileName = "ChmpSlct_BlindPick";
            }
            else 
            {
                SoundFileName = "ChmpSlct_Odin_BlindPick";
            }

            return SoundFileName;
        }

        public static void PlayAmbientChampSelectSound(string SelectedSound)
        {
            Client.AmbientSoundPlayer.Source = new Uri(Path.Combine(Client.ExecutingDirectory, "ambient", SelectedSound + ".mp3"));
        }
    }
}
