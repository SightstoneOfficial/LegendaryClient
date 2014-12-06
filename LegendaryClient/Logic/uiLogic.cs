#region

using LegendaryClient.Windows;

#endregion

namespace LegendaryClient.Logic
{
    internal class UiLogic
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