using LegendaryClient.Logic;
using System.Windows;
using System.Windows.Controls;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for CreateSummonerNameOverlay.xaml
    /// </summary>
    public partial class CreateSummonerNameOverlay : Page
    {
        public CreateSummonerNameOverlay()
        {
            InitializeComponent();
            MessageTextBox.Text = "Your Summoner Name is how other players will know you.\n   * Must be 3-16 characters in length\n   * Must not include the word \"Riot\" (reserved for Riot employees)";
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (await Client.PVPNet.IsNameValidAndAvailable(UsernameTextBox.Text))
            {
                await Client.PVPNet.CreateDefaultSummoner(UsernameTextBox.Text);
                Client.OverlayContainer.Visibility = Visibility.Hidden;
                Client.done = true;
            }
            else
            {
                MessageBox.Show("That username is invalid.");
            }
        }
    }
}