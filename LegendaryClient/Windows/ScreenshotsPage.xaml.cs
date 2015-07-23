using LegendaryClient.Controls;
using LegendaryClient.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.Riot.Platform;
using RtmpSharp.IO;
using RtmpSharp.Messaging;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for ScreenshotsPage.xaml
    /// </summary>
    public partial class ScreenshotsPage
    {
        public ScreenshotsPage()
        {
            InitializeComponent();
        }
    }
}