#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Game;
using Brush = System.Windows.Media.Brush;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

#endregion

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for Ingame.xaml
    /// </summary>
    public partial class Ingame
    {
        private PlatformGameLifecycleDTO _game;
        private string _user;
        //private static readonly ILog log = LogManager.GetLogger(typeof(InGame));
        public Ingame()
        {
            InitializeComponent();
            Change();
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        public void Update(PlatformGameLifecycleDTO currentGame, string username)
        {
            _user = username;
            _game = currentGame;
            BlueBansLabel.Visibility = Visibility.Hidden;
            PurpleBansLabel.Visibility = Visibility.Hidden;
            PurpleBanListView.Items.Clear();
            BlueBanListView.Items.Clear();

            BlueListView.Items.Clear();
            PurpleListView.Items.Clear();

            ImageGrid.Children.Clear();

            var allParticipants = new List<Participant>(currentGame.Game.TeamOne.ToArray());
            allParticipants.AddRange(currentGame.Game.TeamTwo);

            var i = 0;
            var y = 0;
            foreach (var part in allParticipants)
            {
                var control = new ChampSelectPlayer();
                if (part is PlayerParticipant)
                {
                    var participant = part as PlayerParticipant;
                    foreach (
                        var championSelect in
                            currentGame.Game.PlayerChampionSelections.Where(
                                championSelect =>
                                    championSelect.SummonerInternalName == participant.SummonerInternalName))
                    {
                        control.KnownPar = true;
                        control._sumName = participant.SummonerInternalName;
                        control._champID = championSelect.ChampionId;
                        control.ChampionImage.Source = champions.GetChampion(championSelect.ChampionId).icon;
                        var uriSource =
                            new Uri(
                                Path.Combine(Client.ExecutingDirectory, "Assets", "spell",
                                    SummonerSpell.GetSpellImageName(Convert.ToInt32(championSelect.Spell1Id))),
                                UriKind.Absolute);
                        control.SummonerSpell1.Source = new BitmapImage(uriSource);
                        uriSource =
                            new Uri(
                                Path.Combine(Client.ExecutingDirectory, "Assets", "spell",
                                    SummonerSpell.GetSpellImageName(Convert.ToInt32(championSelect.Spell2Id))),
                                UriKind.Absolute);
                        control.SummonerSpell2.Source = new BitmapImage(uriSource);

                        #region Generate Background

                        var m = new Image();
                        Panel.SetZIndex(m, -2);
                        m.Stretch = Stretch.None;
                        m.Width = 100;
                        m.Opacity = 0.50;
                        m.HorizontalAlignment = HorizontalAlignment.Left;
                        m.VerticalAlignment = VerticalAlignment.Stretch;
                        m.Margin = new Thickness(y++*100, 0, 0, 0);
                        var cropRect = new Rectangle(new Point(100, 0), new Size(100, 560));
                        var src =
                            System.Drawing.Image.FromFile(Path.Combine(Client.ExecutingDirectory, "Assets",
                                "champions", champions.GetChampion(championSelect.ChampionId).portraitPath)) as
                                Bitmap;
                        var target = new Bitmap(cropRect.Width, cropRect.Height);

                        using (var g = Graphics.FromImage(target))
                        {
                            if (src != null)
                            {
                                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                    cropRect,
                                    GraphicsUnit.Pixel);
                            }
                        }

                        m.Source = Client.ToWpfBitmap(target);
                        ImageGrid.Children.Add(m);

                        #endregion Generate Background
                    }

                    control.PlayerName.Content = participant.SummonerName;

                    if (participant.TeamParticipantId != null)
                    {
                        var values = BitConverter.GetBytes((double) participant.TeamParticipantId);
                        if (!BitConverter.IsLittleEndian) Array.Reverse(values);

                        var r = values[2];
                        var b = values[3];
                        var g = values[4];

                        var myColor = Color.FromArgb(r, b, g);

                        var converter = new BrushConverter();
                        var brush = (Brush) converter.ConvertFromString("#" + myColor.Name);
                        control.TeamRectangle.Fill = brush;
                        control.TeamRectangle.Visibility = Visibility.Visible;
                    }
                }

                i++;
                if (i <= 5)
                {
                    BlueListView.Items.Add(control);
                }
                else
                {
                    PurpleListView.Items.Add(control);
                }
            }

            if (currentGame.Game.BannedChampions.Count > 0)
            {
                BlueBansLabel.Visibility = Visibility.Visible;
                PurpleBansLabel.Visibility = Visibility.Visible;
            }

            foreach (var x in currentGame.Game.BannedChampions)
            {
                var champImage = new Image
                {
                    Height = 58,
                    Width = 58,
                    Source = champions.GetChampion(x.ChampionId).icon
                };
                if (x.TeamId == 100)
                {
                    BlueBanListView.Items.Add(champImage);
                }
                else
                {
                    PurpleBanListView.Items.Add(champImage);
                }
            }

            try
            {
                string mmrJson;
                var url = Client.Region.SpectatorLink + "consumer/getGameMetaData/" + Client.Region.InternalName +
                          "/" + currentGame.Game.Id + "/token";
                using (var client = new WebClient())
                {
                    mmrJson = client.DownloadString(url);
                }

                var serializer = new JavaScriptSerializer();
                var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(mmrJson);
                MMRLabel.Content = "≈" + deserializedJson["interestScore"];
            }
            catch
            {
                MMRLabel.Content = "N/A";
            }
        }

        private void GameScouter_Click(object sender, RoutedEventArgs e)
        {
            var scouter = new GameScouter();
            scouter.LoadScouter(_user);
            scouter.Show();
            scouter.Activate();
        }

        private void SpectateButton_Click(object sender, RoutedEventArgs e)
        {
            var ip = _game.PlayerCredentials.ObserverServerIp;
            var key = _game.PlayerCredentials.ObserverEncryptionKey;
            var gameId = _game.PlayerCredentials.GameId;
            Client.LaunchSpectatorGame(ip, key, (int) gameId, Client.Region.InternalName);
        }
    }
}