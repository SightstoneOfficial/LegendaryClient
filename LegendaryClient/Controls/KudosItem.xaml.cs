namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for KudosItem.xaml
    /// </summary>
    public partial class KudosItem
    {
        public KudosItem(string type, string amount)
        {
            InitializeComponent();

            TypeLabel.Content = type;
            AmountLabel.Content = amount;
        }
    }
}