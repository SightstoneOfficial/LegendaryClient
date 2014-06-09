using LegendaryClient.Logic;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Windows;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for JoinQueue.xaml
    /// </summary>
    public partial class JoinQueue : UserControl
    {
        public JoinQueue()
        {
            InitializeComponent();
            if (QueueLabel.Content == "matching-queue-groupfinder-5x5-game-queue")
            {
                TeamQueueButton.Visibility = Visibility.Hidden;
                QueueButton.Visibility = Visibility.Hidden;
                QueueLabel.Content = "Team Builder";
                AmountInQueueLabel.Visibility = Visibility.Hidden;
                WaitTimeLabel.Visibility = Visibility.Hidden;
            }
        }
        
    }
}