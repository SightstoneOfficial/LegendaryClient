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
        public static ProfilePage Profile;
        public static MainPage MainPage = new MainPage();

        internal static void UpdateMainPage()
        {
            MainPage.ChangeSpectatorRegion(Client.Region);
            Client.SwitchPage(MainPage);
        }
    }
}