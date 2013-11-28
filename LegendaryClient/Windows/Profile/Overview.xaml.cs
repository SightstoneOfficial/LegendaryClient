using LegendaryClient.Logic;
using System.Windows.Controls;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    public partial class Overview : Page
    {
        public Overview()
        {
            InitializeComponent();
        }

        public void Update(double AccountId)
        {
            //Client.PVPNet.RetrievePlayerStatsByAccountId(accountId, "CURRENT", PlayerLifetimeStats.Callback
        }
    }
}