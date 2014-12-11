#region

using LegendaryClient.Windows;

#endregion

namespace LegendaryClient.Logic
{
    /// <summary>
    ///     Any logic for pages that are called from multiple points
    /// </summary>
    internal class uiLogic
    {
        public static ProfilePage Profile = new ProfilePage();
        public static MainPage MainPage = new MainPage();

        public static void CreateProfile(string name)
        {
            Profile.ProfileCreate(name);
        }

        internal static void UpdateProfile(string name)
        {
            Profile.GetSummonerProfile(name);
            Client.SwitchPage(Profile);
        }

        internal static void UpdateMainPage()
        {
            MainPage.ChangeSpectatorRegion(Client.Region);
            Client.SwitchPage(MainPage);
        }
    }
}