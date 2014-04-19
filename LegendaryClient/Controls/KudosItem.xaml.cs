using System.Windows.Controls;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for KudosItem.xaml
    /// </summary>
    public partial class KudosItem : UserControl
    {
        public KudosItem(string Type, string Amount)
        {
            InitializeComponent();

            TypeLabel.Content = Type;
            AmountLabel.Content = Amount;
        }
    }
}