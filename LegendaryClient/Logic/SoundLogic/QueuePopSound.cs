#region

using System;
using System.IO;

#endregion

namespace LegendaryClient.Logic.SoundLogic
{
    public class QueuePopSound
    {

        public static void PlayQueuePopSound()
        {
            Client.AmbientSoundPlayer.Source =
                new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "matchmakingqueued.mp3"));
            Client.AmbientSoundPlayer.Play();
        }
    }
}