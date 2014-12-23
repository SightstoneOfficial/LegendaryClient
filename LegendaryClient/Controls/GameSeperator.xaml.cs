#region

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LegendaryClient.Logic;
using LegendaryClient.Windows;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for GameSeperator.xaml
    /// </summary>
    public partial class GameSeperator
    {
        private readonly List<JoinQueue> _myItems = new List<JoinQueue>();
        private readonly ListBox _myQueueBox;
        private readonly List<object> _tempList = new List<object>();
        private bool _rotated;

        public GameSeperator(ListBox myQueueBox)
        {
            InitializeComponent();
            _myQueueBox = myQueueBox;
        }

        //Kinda gross, will prob think of a better way to do later.
        private void Polygon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _myQueueBox.SelectedIndex = _myQueueBox.Items.IndexOf(this);
            if (!_rotated && PlayPage.DoneLoading)
            {
                var transform = new RotateTransform(90, 25, 40);
                Triangle.RenderTransform = transform;

                _rotated = !_rotated;
                for (var i = _myQueueBox.SelectedIndex + 1; i < _myQueueBox.Items.Count; i++)
                    _tempList.Add(_myQueueBox.Items.GetItemAt(i));

                foreach (var t in _tempList)
                    _myQueueBox.Items.Remove(t);

                foreach (var t in _myItems)
                    _myQueueBox.Items.Add(t);

                foreach (var t in _tempList)
                    _myQueueBox.Items.Add(t);

                _tempList.Clear();
            }
            else if (PlayPage.DoneLoading)
            {
                var transform = new RotateTransform(0, 25, 40);
                Triangle.RenderTransform = transform;
                _rotated = !_rotated;
                foreach (var t in _myItems)
                    _myQueueBox.Items.Remove(t);
            }
        }

        public void Add(JoinQueue item)
        {
            _myItems.Add(item);
        }

        public List<JoinQueue> GetItems()
        {
            return _myItems;
        }

        internal async void UpdateLabels()
        {
            try
            {
                foreach (var item in _myItems.Where(item => item != null && Client.IsOnPlayPage))
                {
                    var t = await Client.PVPNet.GetQueueInformation(item.QueueId);
                    item.AmountInQueueLabel.Content = "People in queue: " + t.QueueLength;
                }

                var amount =
                    _myItems.Sum(
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