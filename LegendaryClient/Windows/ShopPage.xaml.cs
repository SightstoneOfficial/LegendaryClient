#region

using System;
using System.Windows;
using Awesomium.Core;
using LegendaryClient.Logic;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for ShopPage.xaml
    /// </summary>
    public partial class ShopPage
    {
        public ShopPage()
        {
            InitializeComponent();
        }

        public async void RefreshBrowser()
        {
            ShopBrowser.Source = new Uri(await Client.PVPNet.GetStoreUrl());
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ShopBrowser.Source = new Uri(await Client.PVPNet.GetStoreUrl());
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshBrowser();
        }

        private void ShopBrowser_NativeViewInitialized(object sender, WebViewEventArgs e)
        {
            JSObject JSHook = ShopBrowser.CreateGlobalJavascriptObject("parentSandboxBridge");
            foreach (String x in JSHook.GetMethodNames())
            {
                Client.Log(x, "JSHook");
            }
            JSHook.Bind("openInventoryBrowser", false, OnItemClick);
            JSHook.Bind("getBuddyList", true, OnRequestBuddies);
        }

        public void OnRequestBuddies(object sender, JavascriptMethodEventArgs e)
        {
            var buddyList = new JSValue[Client.AllPlayers.Count];

            int i = 0;
            foreach (var x in Client.AllPlayers)
            {
                var buddy = new JSObject();
                buddy["name"] = new JSValue(x.Value.Username);
                buddy["summonerId"] = new JSValue(x.Key.Replace("sum", ""));
                buddy["isMutualFriend"] = new JSValue(true);
                buddyList[i++] = buddy;
            }

            e.Result = new JSValue(buddyList);
        }

        public void OnItemClick(object sender, JavascriptMethodEventArgs e)
        {
            if (e.Arguments.Length <= 0)
                return;

            string Champion = e.Arguments[0];
            string Skin = e.Arguments[1];
            int ChampionId = Convert.ToInt32(Champion.Replace("champions_", ""));
            if (Skin != "null")
            {
                int SkinID = Convert.ToInt32(Skin.Replace("championsskin_", ""));
                Client.OverlayContainer.Content = new ChampionDetailsPage(ChampionId, SkinID).Content;
            }
            else
            {
                Client.OverlayContainer.Content = new ChampionDetailsPage(ChampionId).Content;
            }
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }
    }
}