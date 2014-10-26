using LegendaryClient.Logic;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Timers;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Windows;
using System.Text.RegularExpressions;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for GameSeperator.xaml
    /// </summary>
    public partial class GameSeperator : UserControl
    {
        private bool rotated = false;
        private List<JoinQueue> myItems = new List<JoinQueue>();
        private ListBox myQueueBox;
        private List<object> templist = new List<object>();

        public GameSeperator(ListBox myQueueBox)
        {
            InitializeComponent();
            this.myQueueBox = myQueueBox;
        }

        //Kinda gross, will prob think of a better way to do later.
        private void Polygon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            myQueueBox.SelectedIndex = myQueueBox.Items.IndexOf(this);
            if (!rotated && PlayPage.DoneLoading)
            {
                RotateTransform transform = new RotateTransform(90, 25, 40);
                this.Triangle.RenderTransform = transform;
                rotated = !rotated;
                for (int i = myQueueBox.SelectedIndex + 1; i < myQueueBox.Items.Count; i++)
                    templist.Add(myQueueBox.Items.GetItemAt(i));
                for (int i = 0; i < templist.Count; i++)
                    myQueueBox.Items.Remove(templist[i]);
                for (int i = 0; i < myItems.Count; i++)
                    myQueueBox.Items.Add(myItems[i]);
                for (int i = 0; i < templist.Count; i++)
                    myQueueBox.Items.Add(templist[i]);
                templist.Clear();
            }
            else if(PlayPage.DoneLoading)
            {
                RotateTransform transform = new RotateTransform(0, 25, 40);
                this.Triangle.RenderTransform = transform;
                rotated = !rotated;
                for (int i = 0; i < myItems.Count; i++)
                {
                    myQueueBox.Items.Remove(myItems[i]);
                }
            }
        }

        public void Add(JoinQueue item)
        {
            myItems.Add(item);
        }

        public List<JoinQueue> getItems()
        {
            return myItems;
        }

        internal async void UpdateLabels()
        {
            foreach (JoinQueue item in myItems)
            {
                if (item != null && Client.IsOnPlayPage)
                {
                    QueueInfo t = await Client.PVPNet.GetQueueInformation(item.queueID);
                    item.AmountInQueueLabel.Content = "People in queue: " + t.QueueLength;
                }
            }
            int amount = 0;
            for (int i = 0; i < myItems.Count; i++)
            {
                amount += int.Parse(Regex.Match(myItems[i].AmountInQueueLabel.Content as string, "\\d+").Value);
            }
            AmountInQueueLabel.Content = "People in queue: " + amount;
        }
    }
}