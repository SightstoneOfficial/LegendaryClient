using LegendaryClient.Logic;
using LegendaryClient.Logic.Player;
using LegendaryClient.Logic.SQLite;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.Riot.Leagues;
using LegendaryClient.Logic.Riot.Platform;

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for MainPagePlayer.xaml
    /// </summary>
    public partial class MainPagePlayer
    {
        private PlayerStatisticsChampSelect stats;
        public string sumName;
        public int champID;
        public bool KnownPar;

        public MainPagePlayer(string sumName = "", int champID = 0, bool known = true)
        {
            InitializeComponent();
            this.sumName = sumName;
            this.champID = champID;
        }
    }
}