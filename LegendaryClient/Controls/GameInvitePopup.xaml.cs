using System.Windows;
using System.Windows.Controls;
using PVPNetConnect.RiotObjects.Gameinvite.Contract;

namespace LegendaryClient.Controls {
    /// <summary>
    /// Interaction logic for GameInvitePopup.xaml
    /// </summary>
    public partial class GameInvitePopup : UserControl {
        public GameInvitePopup(InvitationRequest stats, Inviter inviter) {
            InitializeComponent();
        }

        private void Accept_Click(object sender, RoutedEventArgs e) {
            
        }

        private void Decline_Click(object sender, RoutedEventArgs e) {
            
        }
    }
}