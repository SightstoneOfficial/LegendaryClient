using System;
using System.Windows;
using Awesomium.Core;
using LegendaryClient.Logic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Properties;
using LegendaryClient.Logic.MultiUser;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for ShopPage.xaml
    /// </summary>
    public partial class ShopPage
    {
        UserClient UserClient = UserList.Users[Client.Current];
        public ShopPage()
        {
            InitializeComponent();
            //Tard();
        }
        async Task Tard()
        {
            while (true)
            {
                ShopBrowser.ExecuteJavascript(
                    "document.getElementsByClassName('search_text').innerHTML = document.getElementsByClassName('search_text').innerHTML.replace('Bard', 'Tard');");
                ShopBrowser.ExecuteJavascript(
                    "document.getElementsByClassName('item_name').innerHTML = document.getElementsByClassName('item_name').innerHTML.replace('Bard', 'Tard');");
                await Task.Delay(1000);
                if (Client.CurrentPage.GetType() != typeof(ShopPage))
                    break;
            }
        }

        public async void RefreshBrowser()
        {
            var uri = await UserClient.calls.GetStoreUrl();
            ShopBrowser.Source = new System.Uri(uri);
            Debug.WriteLine(uri);
            //Client.Log(await RiotCalls.GetStoreUrl());

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
            foreach (string x in jsHook.GetMethodNames())
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