using LegendaryClient.Logic;
using System.Windows;
using System.Windows.Controls;
using PVPNetConnect.RiotObjects.Platform.Trade;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for TradeControl.xaml
    /// </summary>
    public partial class TradeControl : UserControl
    {
        public TradeControl()
        {
            InitializeComponent();
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            TradeContractDTO TradeDTO = this.Tag as TradeContractDTO;
            await Client.PVPNet.AcceptTrade(TradeDTO.RequesterInternalSummonerName, (int)TradeDTO.RequesterChampionId);
        }

        private async void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            await Client.PVPNet.DeclineTrade();
            this.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}