#region

using System;
using System.IO;
using System.Windows;
using System.Xml;
using System.Linq;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Windows;
using LegendaryClient.Logic.Riot.Team;
using agsXMPP.protocol.client;
using agsXMPP;
using LegendaryClient.Logic.Riot.Platform;
using LegendaryClient.Logic.Riot.Leagues;

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
                Client.Log("FriendRequest stuff coming");
                Client.Log(message.From.User.Replace("sum", string.Empty));
                var summonerId = message.From.User.Replace("sum", string.Empty).ToInt();
                var summonerName = await RiotCalls.GetSummonerNames(new double[] {summonerId});
                var playerInfo = await RiotCalls.GetSummonerByName(summonerName[0]);
                Client.Log(playerInfo.Name);

                SummonerLeaguesDTO playerLeagues =
                        await RiotCalls.GetAllLeaguesForPlayer(summonerId);
                string rank = string.Empty;
                foreach (LeagueListDTO l in playerLeagues.SummonerLeagues.Where(l => l.Queue == "RANKED_SOLO_5x5"))
                    rank = l.Tier + " " + l.RequestorsRank;
                if (string.IsNullOrEmpty(rank))
                    rank = "Unranked";
                NotificationTextBox.Text = string.Format(@"{0} would like to have you as a friend
Level: {1}
Rank: {2}", playerInfo.Name, playerInfo.SummonerLevel, rank);
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