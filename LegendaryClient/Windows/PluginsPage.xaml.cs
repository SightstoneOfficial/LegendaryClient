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
using System.IO;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for PluginsPage.xaml
    /// </summary>
    public partial class PluginsPage
    {
		private Scripting_Environment.Plugin_Core Core = new Scripting_Environment.Plugin_Core();
		private List<string> verifiedPlugins;
        public PluginsPage()
        {
            InitializeComponent();
			verifiedPlugins = fetchPluginList();
        }
        private void LoadScript_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog myFile = new Microsoft.Win32.OpenFileDialog();
            if (myFile.ShowDialog().GetValueOrDefault() == false)
                return;
            var PluginPath = myFile.FileName;
			Core.LoadScript(PluginPath, myFile.SafeFileName);
			foreach(var Plugin in Core.getAllLoadedPlugins()) //this needs more Improvement performance wise
			{
				Plugin.isVerifed = isVerified(PluginPath);
			}
			Core.runAll();
			
        }
		private List<string> fetchPluginList()
		{
			try
			{
				WebClient client = new WebClient();
				Stream stream = client.OpenRead("https://raw.githubusercontent.com/LegendaryClient/LegendaryClient/gh-pages/Plugins");
				StreamReader reader = new StreamReader(stream);
				string content = reader.ReadToEnd();
				var Plugins = content.Split('\n').ToList();
				Plugins.RemoveAll(x => x.StartsWith("#"));
				return Plugins;
			}
			catch
			{
				return new List<string>();
			}
		}
		private bool isVerified(string PluginPath)
		{
			Logic.Crypto.Sha1 shaProvider = new Logic.Crypto.Sha1();
			var content = File.ReadAllText(PluginPath);
			var hash = shaProvider.Hash(content);
			if (verifiedPlugins.Contains(hash))
				return true;
			return false;
		}
    }
}