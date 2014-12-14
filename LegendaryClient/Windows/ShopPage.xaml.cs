#region

using System;
using System.Windows;
using Awesomium.Core;
using LegendaryClient.Logic;
using LegendaryClient.Properties;

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

        public async void RefreshBrowser()
        {
            ShopBrowser.Source = new Uri(await Client.PVPNet.GetStoreUrl());
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshBrowser();
            ShopBrowser.Reload(false);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshBrowser();
        }

        private void ShopBrowser_NativeViewInitialized(object sender, WebViewEventArgs e)
        {
            JSObject jsHook = ShopBrowser.CreateGlobalJavascriptObject("parentSandboxBridge");
            foreach (String x in jsHook.GetMethodNames())
                Client.Log(x, "JSHook");

            jsHook.Bind("openInventoryBrowser", false, OnItemClick);
            jsHook.Bind("getBuddyList", true, OnRequestBuddies);
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

            string champion = e.Arguments[0];
            string skin = e.Arguments[1];
            int championId = Convert.ToInt32(champion.Replace("champions_", ""));
            if (skin != "null")
            {
                int skinId = Convert.ToInt32(skin.Replace("championsskin_", ""));
                Client.OverlayContainer.Content = new ChampionDetailsPage(championId, skinId).Content;
            }
            else
                Client.OverlayContainer.Content = new ChampionDetailsPage(championId).Content;

            Client.OverlayContainer.Visibility = Visibility.Visible;
        }
    }
}