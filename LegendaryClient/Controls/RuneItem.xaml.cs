namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for RuneItem.xaml
    /// </summary>
    public partial class RuneItem
    {
        public RuneItem()
        {
            InitializeComponent();
            Owned = 0;
            Used = 0;
        }

        public int Owned { get; set; }
        public int Used { get; set; }
    }
}