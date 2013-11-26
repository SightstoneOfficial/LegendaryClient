using LegendaryClient.Logic;
using System;
using System.Windows.Controls;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ShopPage.xaml
    /// </summary>
    public partial class ShopPage : Page
    {
        public ShopPage()
        {
            InitializeComponent();
            RefreshBrowser();
        }

        public async void RefreshBrowser()
        {
            ShopBrowser.Source = new Uri(await Client.PVPNet.GetStoreUrl());
        }
    }
}