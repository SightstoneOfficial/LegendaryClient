namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for BotControl.xaml
    /// </summary>
    public partial class BotControl
    {
        public int difficulty {get; set;}
        public bool blueSide { get; set; }
        public BotControl()
        {
            InitializeComponent();
        }
    }
}