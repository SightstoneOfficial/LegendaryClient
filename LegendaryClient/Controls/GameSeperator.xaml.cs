using LegendaryClient.Logic;
using LegendaryClient.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for GameSeperator.xaml
    /// </summary>
    public partial class GameSeperator
    {
        private readonly List<JoinQueue> myItems = new List<JoinQueue>();
        private readonly ListBox myQueueBox;
        private readonly List<object> tempList = new List<object>();
        private bool rotated;

        public GameSeperator(ListBox myQueueBox)
        {
            InitializeComponent();
            this.myQueueBox = myQueueBox;
        }

        //Kinda gross, will prob think of a better way to do later.
        private void Polygon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            myQueueBox.SelectedIndex = myQueueBox.Items.IndexOf(this);
            if (!rotated && PlayPage.DoneLoading)
            {
                var transform = new RotateTransform(90, 25, 40);
                Triangle.RenderTransform = transform;

                rotated = !rotated;
                for (var i = myQueueBox.SelectedIndex + 1; i < myQueueBox.Items.Count; i++)
                    tempList.Add(myQueueBox.Items.GetItemAt(i));

                foreach (var t in tempList)
                    myQueueBox.Items.Remove(t);

                foreach (var t in myItems)
                    myQueueBox.Items.Add(t);

                foreach (var t in tempList)
                    myQueueBox.Items.Add(t);

                tempList.Clear();
            }
            else if (PlayPage.DoneLoading)
            {
                var transform = new RotateTransform(0, 25, 40);
                Triangle.RenderTransform = transform;
                rotated = !rotated;
                foreach (var t in myItems)
                    myQueueBox.Items.Remove(t);
            }
        }

        public void Add(JoinQueue item)
        {
            myItems.Add(item);
        }

        public List<JoinQueue> GetItems()
        {
            return myItems;
        }

        internal async void UpdateLabels()
        {
            try
            {
                foreach (var item in myItems.Where(item => item != null && Client.IsOnPlayPage))
                {
                    var t = await Client.PVPNet.GetQueueInformation(item.QueueId);
                    item.AmountInQueueLabel.Content = "People in queue: " + t.QueueLength;
                }

                var amount =
                    myItems.Sum(
                        t =>
                            t.AmountInQueueLabel != null
                                ? int.Parse(Regex.Match(t.AmountInQueueLabel.Content as string, "\\d+").Value)
                                : 0);
                AmountInQueueLabel.Content = "People in queue: " + amount;
            }
            catch
            {
                Client.Log("Items collection was most likely modified.");
            }
        }
    }
}