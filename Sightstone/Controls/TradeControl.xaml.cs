#region

using System.Windows;
using Sightstone.Logic;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using Sightstone.Logic.MultiUser;

#endregion

namespace Sightstone.Controls
{
    /// <summary>
    ///     Interaction logic for TradeControl.xaml
    /// </summary>
    public partial class TradeControl
    {
        static UserClient UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
        public TradeControl()
        {
            InitializeComponent();
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            var tradeDTO = Tag as TradeContractDTO;
            if (tradeDTO != null)
                await
                    UserClient.calls.AcceptTrade(tradeDTO.RequesterInternalSummonerName, (int)tradeDTO.RequesterChampionId);
        }

        private async void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            await UserClient.calls.DeclineTrade();
            Visibility = Visibility.Hidden;
        }
    }
}