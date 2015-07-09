using LegendaryClient.Logic;
using System.Windows;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.MultiUser;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for CreateSummonerNameOverlay.xaml
    /// </summary>
    public partial class CreateSummonerNameOverlay
    {
        static UserClient UserClient;
        public CreateSummonerNameOverlay(UserClient client)
        {
            InitializeComponent();
            UserClient = client;
            MessageTextBox.Text =
                "Your Summoner Name is how other players will know you.\n   * Must be 3-16 characters in length\n   * Letters, numbers and spaces are allowed\n   * Must not contain profanity\n   * Must not include the word \"Riot\" (reserved for Riot employees)";
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text.Length > 0)
            {
                if (await UserClient.calls.CreateDefaultSummoner(UsernameTextBox.Text) != null)
                {
                    Client.OverlayContainer.Visibility = Visibility.Hidden;
                    UserClient.done = true;
                }
                else
                    MessageBox.Show("That username is invalid.");
            }
            else
            {
                MessageBox.Show("Please enter a username.");
            }
        }
    }
}