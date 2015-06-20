#region

using System;
using System.IO;
using System.Windows;
using System.Xml;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Windows;
using LegendaryClient.Logic.Riot.Team;
using agsXMPP.protocol.client;
using agsXMPP;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for NotificationPopup.xaml
    /// </summary>
    public partial class FriendInvite
    {
        private readonly ChatSubjects _subject;
        private readonly Jid jid;
        public FriendInvite(ChatSubjects subject, agsXMPP.protocol.client.Presence message)
        {
            InitializeComponent();
            if (subject == ChatSubjects.XMPP_SUBSCRIBE)
            {
                jid = message.From;
                Client.Log(jid.Bare);
                load(message);
            }

        }
        private async void load(agsXMPP.protocol.client.Presence message)
        {
            try
            {
                Client.Log(message.From.User.Replace("sum", string.Empty));
                var x = await RiotCalls.GetAllPublicSummonerDataByAccount(message.From.User.Replace("sum", string.Empty).ToInt());
                Client.Log(x.Summoner.InternalName);
                NotificationTextBox.Text = string.Format(
                    @"{0} would like to have you as a friend
Level: {1}
Rank: {2}", x.Summoner.InternalName, x.SummonerLevel.Level, x.Summoner.SeasonTwoTier);
            }
            catch 
            {
                NotificationTextBox.Text = string.Format(@"{0} would like to have you as a friend", message.From.User);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            Client.PresManager.RefuseSubscriptionRequest(jid);
            Client.PresManager.Unsubscribe(jid);
            Visibility = Visibility.Hidden;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Client.PresManager.ApproveSubscriptionRequest(jid);
            Client.PresManager.Subscribe(jid);
            Visibility = Visibility.Hidden;
        }
    }
}