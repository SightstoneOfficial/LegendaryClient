using LegendaryClient.Logic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for NotificationChatPlayer.xaml
    /// </summary>
    public partial class NotificationChatPlayer : UserControl
    {
        public string PlayerName;
        public NotificationChatPlayer()
        {
            InitializeComponent();
            Blink();
        }

        public void Blink()
        {
            DoubleAnimation fadingAnimation = new DoubleAnimation();
            fadingAnimation.From = 0.6;
            fadingAnimation.To = 0;
            fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            fadingAnimation.RepeatBehavior = RepeatBehavior.Forever;
            fadingAnimation.Completed += (eSender, eArgs) =>
            {
                fadingAnimation = new DoubleAnimation();
                fadingAnimation.From = 0;
                fadingAnimation.To = 0.6;
                fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            };

            BlinkRectangle.BeginAnimation(Image.OpacityProperty, fadingAnimation);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.ChatItem != null)
            {
                Client.MainGrid.Children.Remove(Client.ChatItem);
                Client.ChatClient.OnMessage -= Client.ChatItem.ChatClient_OnMessage;
                Client.ChatItem = null;
            }

            Client.ChatListView.Items.Remove(this);
        }
    }
}