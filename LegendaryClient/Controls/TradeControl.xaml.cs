#region

using System.Windows;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.Riot.Platform;
using LegendaryClient.Logic.MultiUser;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for TradeControl.xaml
    /// </summary>
    public partial class TradeControl
    {
        static UserClient UserClient = UserList.users[Client.Current];
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