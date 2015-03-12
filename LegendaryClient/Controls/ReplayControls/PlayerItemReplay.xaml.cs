#region

using System.Collections.Generic;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for PlayerItemReplay.xaml
    /// </summary>
    public partial class PlayerItemReplay
    {
        public PlayerItemReplay()
        {
            InitializeComponent();
        }

        public List<object> getChildElements()
        {
            var elements = new List<object>
            {
                ChampionIcon,
                PlayerNameLabel,
                File,
                gameItem1,
                gameItem2,
                gameItem3,
                gameItem4,
                gameItem5,
                gameTrinket,
                KDA
            };

            return elements;
        }
    }
}