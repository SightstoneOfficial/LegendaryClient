using System;
using System.IO;
using Sightstone.Logic.SQLite;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Logic.SoundLogic
{
    public class SelectChampion
    {
        public static int SelectChamp(int champ)
        {
            var locale = (UserList.Users[Client.CurrentServer])[Client.CurrentUser].Region.Locale;
            Client.SoundPlayer.Source =
                new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", locale, "champions", champions.GetChampion(champ).name + ".mp3"));
            Client.SoundPlayer.Play();


            int hi = champ;

            return hi;
        }
    }
}