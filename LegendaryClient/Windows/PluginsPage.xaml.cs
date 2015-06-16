using LegendaryClient.Controls;
using LegendaryClient.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.Riot.Platform;
using RtmpSharp.IO;
using RtmpSharp.Messaging;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for PluginsPage.xaml
    /// </summary>
    public partial class PluginsPage
    {
		private Scripting_Environment.Plugin_Core Core = new Scripting_Environment.Plugin_Core();
        public PluginsPage()
        {
            InitializeComponent();
        }
        private void LoadScript_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog myFile = new Microsoft.Win32.OpenFileDialog();
            if (myFile.ShowDialog().GetValueOrDefault() == false)
                return;
            var PluginPath = myFile.FileName;
			Core.LoadScript(PluginPath, myFile.SafeFileName);
			Core.runAll();
        }
    }
}