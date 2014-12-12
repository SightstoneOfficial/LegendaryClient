#region

using System;
using System.IO;

#endregion

namespace LegendaryClient.Logic.SoundLogic
{
    public class AmbientChampSelect
    {
        public static string CurrentQueueToSoundFile(double queueId)
        {
            string soundFileName;
            if (queueId == 2)
                soundFileName = "ChmpSlct_BlindPick";
            else if (queueId == 4)
                soundFileName = "ChmpSlct_DraftMode";
            else if (queueId == 61)
                soundFileName = "ChmpSlct_DraftMode";
            else if (queueId == 31)
                soundFileName = "ChmpSlct_BlindPick";
            else if (queueId == 32)
                soundFileName = "ChmpSlct_BlindPick";
            else if (queueId == 42)
                soundFileName = "ChmpSlct_DraftMode";
            else if (queueId == 65)
                soundFileName = "howlingabyss_champselect";
            else if (queueId == 32)
                soundFileName = "ChmpSlct_BlindPick";
            else
                soundFileName = "ChmpSlct_Odin_BlindPick";

            return soundFileName;
        }

        public static void PlayAmbientChampSelectSound(string selectedSound)
        {
            Client.AmbientSoundPlayer.Source =
                new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "ambient", selectedSound + ".mp3"));
            Client.AmbientSoundPlayer.Play();
        }
    }
}