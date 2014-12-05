using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegendaryClient.Windows;


namespace LegendaryClient.Logic
{
    class uiLogic
    {
        public static ProfilePage Profile = new ProfilePage();

        public static void CreateProfile(string name)
        {
            Profile.ProfileCreate(name);
        }
        internal static void UpdateProfile(string name)
        {
            Profile.GetSummonerProfile(name);
            Client.SwitchPage(Profile);
        }
    }
}
