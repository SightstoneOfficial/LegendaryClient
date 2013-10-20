using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using LegendaryClient.Logic;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        List<string> CFGLocations = new List<string>();
        public SettingsPage()
        {
            InitializeComponent();
            GetCFGFiles();
        }

        private void GetCFGFiles()
        {
            FilesListView.Items.Clear();
            CFGLocations.Clear();
            string CFGLocation = Path.Combine(Client.ExecutingDirectory, "lol_client", "DATA", "CFG");
            foreach (string file in Directory.EnumerateFiles(CFGLocation, "*.*", SearchOption.AllDirectories))
            {
                if (!file.EndsWith(".xml"))
                {
                    FilesListView.Items.Add(file.Replace(CFGLocation, ""));
                    CFGLocations.Add(file);
                }
            }
        }

        private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CFGListView.Items.Clear();
            if (FilesListView.SelectedIndex != -1)
            {
                string FileLocation = CFGLocations[FilesListView.SelectedIndex];
                string[] lines = File.ReadAllLines(FileLocation);

                foreach (string line in lines)
                {
                    string TrimmedLine = (string)line.Clone(); //Duplicate it (so original chars arent removed with trim) then trim it
                    TrimmedLine = TrimmedLine.Trim();
                    if (!String.IsNullOrEmpty(TrimmedLine))
                    {
                        if (!line.StartsWith(";"))
                        {
                            if (line.StartsWith("["))
                            {
                                SettingsItem item = new SettingsItem()
                                {
                                    Setting = line.Replace("[", "").Replace("]", ""),
                                    Value = "",
                                    File = ""
                                };
                                CFGListView.Items.Add(item);
                                
                            }
                            else
                            {
                                string[] splitSetting = line.Split('=');
                                SettingsItem item = new SettingsItem()
                                {
                                    Setting = splitSetting[0],
                                    Value = splitSetting[1],
                                    File = FileLocation
                                };
                                CFGListView.Items.Add(item);
                            }
                        }
                    }
                }

                if (double.IsNaN(SettingHeader.Width))
                {
                    SettingHeader.Width = SettingHeader.ActualWidth;
                }
                SettingHeader.Width = double.NaN;

            }
        }

    }

    public class SettingsItem
    {
        public string Setting { get; set; }
        public string Value { get; set; }
        public string File { get; set; }
    }
}
