using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for Ingame.xaml
    /// </summary>
    public partial class Ingame : Page
    {
        public Ingame()
        {
            InitializeComponent();
        }

        public void Update(PlatformGameLifecycleDTO CurrentGame)
        {
            BlueBansLabel.Visibility = System.Windows.Visibility.Hidden;
            PurpleBansLabel.Visibility = System.Windows.Visibility.Hidden;
            PurpleBanListView.Items.Clear();
            BlueBanListView.Items.Clear();

            BlueListView.Items.Clear();
            PurpleListView.Items.Clear();

            ImageGrid.Children.Clear();

            List<Participant> AllParticipants = new List<Participant>(CurrentGame.Game.TeamOne.ToArray());
            AllParticipants.AddRange(CurrentGame.Game.TeamTwo);

            int i = 0;
            int y = 0;
            foreach (Participant part in AllParticipants)
            {
                ChampSelectPlayer control = new ChampSelectPlayer();
                if (part is PlayerParticipant)
                {
                    PlayerParticipant participant = part as PlayerParticipant;
                    foreach (PlayerChampionSelectionDTO championSelect in CurrentGame.Game.PlayerChampionSelections)
                    {
                        if (championSelect.SummonerInternalName == participant.SummonerInternalName)
                        {
                            control.ChampionImage.Source = champions.GetChampion(championSelect.ChampionId).icon;
                            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(Convert.ToInt32(championSelect.Spell1Id))), UriKind.Absolute);
                            control.SummonerSpell1.Source = new BitmapImage(uriSource);
                            uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(Convert.ToInt32(championSelect.Spell2Id))), UriKind.Absolute);
                            control.SummonerSpell2.Source = new BitmapImage(uriSource);

                            #region Generate Background
                            Image m = new Image();
                            Canvas.SetZIndex(m, -2);
                            m.Stretch = Stretch.None;
                            m.Width = 100;
                            m.Opacity = 0.50;
                            m.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                            m.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                            m.Margin = new System.Windows.Thickness(y++ * 100, 0, 0, 0);
                            System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle(new System.Drawing.Point(100, 0), new System.Drawing.Size(100, 560));
                            System.Drawing.Bitmap src = System.Drawing.Image.FromFile(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(championSelect.ChampionId).portraitPath)) as System.Drawing.Bitmap;
                            System.Drawing.Bitmap target = new System.Drawing.Bitmap(cropRect.Width, cropRect.Height);

                            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(target))
                            {
                                g.DrawImage(src, new System.Drawing.Rectangle(0, 0, target.Width, target.Height),
                                                cropRect,
                                                System.Drawing.GraphicsUnit.Pixel);
                            }

                            m.Source = ToWpfBitmap(target);
                            ImageGrid.Children.Add(m);
                            #endregion
                        }
                    }

                    control.PlayerName.Content = participant.SummonerName;

                    if (participant.TeamParticipantId != null)
                    {
                        byte[] values = BitConverter.GetBytes((double)participant.TeamParticipantId);
                        if (!BitConverter.IsLittleEndian) Array.Reverse(values);

                        byte r = values[2];
                        byte b = values[3];
                        byte g = values[4];

                        System.Drawing.Color myColor = System.Drawing.Color.FromArgb(r, b, g);

                        var converter = new System.Windows.Media.BrushConverter();
                        var brush = (Brush)converter.ConvertFromString("#" + myColor.Name);
                        control.TeamRectangle.Fill = brush;
                        control.TeamRectangle.Visibility = System.Windows.Visibility.Visible;
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

            if (CurrentGame.Game.BannedChampions.Count > 0)
            {
                BlueBansLabel.Visibility = System.Windows.Visibility.Visible;
                PurpleBansLabel.Visibility = System.Windows.Visibility.Visible;
            }

            foreach (var x in CurrentGame.Game.BannedChampions)
            {
                ListViewItem item = new ListViewItem();
                Image champImage = new Image();
                champImage.Height = 58;
                champImage.Width = 58;
                champImage.Source = champions.GetChampion(x.ChampionId).icon;
                item.Content = champImage;
                if (x.TeamId == 100)
                {
                    BlueBanListView.Items.Add(item);
                }
                else
                {
                    PurpleBanListView.Items.Add(item);
                }
            }

            try
            {
                string mmrJSON = "";
                string url = Client.Region.SpectatorLink + "consumer/getGameMetaData/" + Client.Region.InternalName + "/" + CurrentGame.Game.Id + "/token";
                using (WebClient client = new WebClient())
                {
                    mmrJSON = client.DownloadString(url);
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(mmrJSON);
                MMRLabel.Content = "≈" + deserializedJSON["interestScore"];
            }
            catch { MMRLabel.Content = "N/A"; }
        }

        public BitmapSource ToWpfBitmap(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
    }
}