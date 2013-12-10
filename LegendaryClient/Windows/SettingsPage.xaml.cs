using LegendaryClient.Logic;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Forms;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        #region DLL Stuff

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(
              string deviceName, int modeNum, ref DEVMODE devMode);

        private const int ENUM_CURRENT_SETTINGS = -1;

        private const int ENUM_REGISTRY_SETTINGS = -2;

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;

            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;

            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        #endregion DLL Stuff

        private List<string> Resolutions = new List<string>();

        public SettingsPage()
        {
            InitializeComponent();
            InsertDefaultValues();

            StatsCheckbox.IsChecked = Properties.Settings.Default.GatherStatistics;
            ErrorCheckbox.IsChecked = Properties.Settings.Default.SendErrors;

            #region AboutTextbox

            AboutTextBox.Text =
@"Copyright (c) 2013-2014, Snowl (David Diaz - d@viddiaz.com)
All rights reserved.

This client is open-source and can be found at http://legendaryclient.com

Thanks to " + Client.LoginPacket.AllSummonerData.Summoner.Name + @". Using this client means the most to me. Thank you very much!

Thanks to all the people at #riotcontrol, especially raler (for providing PVPNetConnect).
Thanks to all the people who done the RTMPS work. Your help has been invaluable.

Thanks Riot for providing a pretty awesome game. It might not be perfect, but I have wasted many hours on it.

Thanks to all the people who have supported me (excluding gort).

Uses Data Dragon QA Interface & l3cdn.

This product is not endorsed, certified or otherwise approved in any way by Riot Games, Inc. or any of its affiliates.

External libraries:
Awesomium
jabber-net
MahApps.Metro
PVPNetConnect
SharpZipLib
sqlite
zlib

Donations are accepted at:
Bitcoin: 1Pq5HWenYoNkHKbKcRjQXshuFeSmkkXH5d
Litecoin: LWMujPiDyfDoQt33cL6FHmMt78fYmjhzGB";

            #endregion AboutTextbox
        }

        public void InsertDefaultValues()
        {
            //Insert resolutions
            DEVMODE vDevMode = new DEVMODE();
            int i = 0;

            while (EnumDisplaySettings(null, i, ref vDevMode))
            {
                if (!Resolutions.Contains(String.Format("{0}x{1}", vDevMode.dmPelsWidth, vDevMode.dmPelsHeight)) && vDevMode.dmPelsWidth >= 1000)
                {
                    Resolutions.Add(String.Format("{0}x{1}", vDevMode.dmPelsWidth, vDevMode.dmPelsHeight));
                    ResolutionComboBox.Items.Add(String.Format("{0}x{1}", vDevMode.dmPelsWidth, vDevMode.dmPelsHeight));
                }
                i++;
            }

            ResolutionComboBox.SelectedIndex = ResolutionComboBox.Items.Count - 1;
            WindowModeComboBox.SelectedIndex = 0;
        }

        private void StatsCheckbox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Properties.Settings.Default.GatherStatistics = (bool)StatsCheckbox.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void ErrorCheckbox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Properties.Settings.Default.SendErrors = (bool)ErrorCheckbox.IsChecked;
            Properties.Settings.Default.Save();
        }
    }
}