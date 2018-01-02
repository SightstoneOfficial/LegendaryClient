using Sightstone.Controls;
using Sightstone.Logic;
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
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using RtmpSharp.IO;
using RtmpSharp.Messaging;

namespace Sightstone.Windows
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
			var loadedPlugin = Core.LoadScript(PluginPath, myFile.SafeFileName);
			Core.runAll();
			PluginList.Items.Add(new test { Enabled = true, Author = "", Description = loadedPlugin.Description, Name = loadedPlugin.Name, Version = "1.0.0.0" });
        }
		public class test
		{
			public bool Enabled;
			public string Name;
			public string Version;
			public string Author;
			public string Description;
		}
    }
}