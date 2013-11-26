using LegendaryClient.Logic;
using System.Windows;
using System.Windows.Controls;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for Masteries.xaml
    /// </summary>
    public partial class Masteries : Page
    {
        public Masteries()
        {
            InitializeComponent();
            int i = 1;
            foreach (var MasteryPage in Client.LoginPacket.AllSummonerData.MasteryBook.BookPages)
            {
                Button b = new Button();
                b.Content = i++;
                b.Width = 28;
                b.Margin = new Thickness(2, 0, 2, 0);
                MasteryPageListView.Items.Add(b);
            }
        }
    }
}