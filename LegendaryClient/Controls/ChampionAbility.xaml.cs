#region

using System;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for ChampionAbility.xaml
    /// </summary>
    public partial class ChampionAbility : IComparable
    {
        public ChampionAbility()
        {
            InitializeComponent();
        }

        public int Order { get; set; }

        public int CompareTo(object ability)
        {
            if (ability is ChampionAbility)
            {
                return Order.CompareTo((ability as ChampionAbility).Order);
            }
            throw new ArgumentException("Passed object is not of type ChampionAbility");
        }
    }
}