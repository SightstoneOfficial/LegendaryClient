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
            var championAbility = ability as ChampionAbility;
            if (championAbility != null)
                return Order.CompareTo(championAbility.Order);

            throw new ArgumentException("Passed object is not of type ChampionAbility");
        }
    }
}