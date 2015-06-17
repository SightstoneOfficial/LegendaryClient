using System;
using System.IO;

namespace LegendaryClient.Logic.SoundLogic
{
    public class QueuePopSound
    {

        public static void PlayQueuePopSound()
        {
            Client.AmbientSoundPlayer.Source =
                new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "matchmakingqueued.mp3"));
            Client.AmbientSoundPlayer.Play();
        }
    }
}