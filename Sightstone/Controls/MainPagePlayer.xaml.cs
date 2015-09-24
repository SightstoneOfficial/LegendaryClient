using Sightstone.Logic;
using Sightstone.Logic.Player;
using Sightstone.Logic.SQLite;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Leagues;
using Sightstone.Logic.Riot.Platform;

namespace Sightstone.Controls
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