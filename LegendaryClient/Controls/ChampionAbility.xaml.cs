using System;
using System.Windows.Controls;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for ChampionAbility.xaml
    /// </summary>
    public partial class ChampionAbility : UserControl, IComparable
    {
        public int Order { get; set; }

        public ChampionAbility()
        {
            InitializeComponent();
        }

        public int CompareTo(object ability)
        {
            if(ability is ChampionAbility) {
                return this.Order.CompareTo((ability as ChampionAbility).Order);
            }
            throw new ArgumentException("Passed object is not of type ChampionAbility");
        }
    }
}