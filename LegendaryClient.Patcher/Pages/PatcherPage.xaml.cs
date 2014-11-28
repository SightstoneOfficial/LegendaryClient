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
using System.Windows.Shapes;
using LegendaryClient.Patcher.Logic;
using System.Web.Script.Serialization;

namespace LegendaryClient.Patcher.Pages
{
    /// <summary>
    /// Interaction logic for PatcherPage.xaml
    /// 
    /// Future patcher that will make it so users will no longer need League of Legends on their PCs
    /// Updates SplashUpdate as well just incase it is needed
    /// </summary>
    public partial class PatcherPage : Page
    {
        String ExecutingDirectory;
        Boolean IsLogVisible;
        public PatcherPage()
        {
            InitializeComponent();

            IsLogVisible = false;
            //Finds where the patcher was started
            if (!File.Exists(System.IO.Path.Combine(ExecutingDirectory, "Patcher.settings")))
            {
                var x = File.Create(System.IO.Path.Combine(ExecutingDirectory, "Patcher.settings"));
                //Client.OverlayGrid.Content
                x.Close();
            }
            else
            {

                File.ReadAllText(System.IO.Path.Combine(ExecutingDirectory, "Patcher.settings"));
                JavaScriptSerializer json = new JavaScriptSerializer();

            }
            


            ExecutingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            if (File.Exists(System.IO.Path.Combine(ExecutingDirectory, "LegendaryClientPatcher.log")))
            {
                File.Delete(System.IO.Path.Combine(ExecutingDirectory, "LegendaryClientPatcher.log"));
            }
            //LogTextBox(CreateConfigurationmanifest());
        }

        /// <summary>
        /// Swiches the command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Switch_Click(object sender, RoutedEventArgs e)
        {
            if (IsLogVisible == true)
            {
                IsLogVisible = false;
                NewsGrid.Visibility = Visibility.Visible;
                LogGrid.Visibility = Visibility.Hidden;
            }
            else if (IsLogVisible == false)
            {
                IsLogVisible = true;
                NewsGrid.Visibility = Visibility.Hidden;
                LogGrid.Visibility = Visibility.Visible;
            }

        }

        /// <summary>
        /// Starts LegendaryClient
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// Highlights UnderButtons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverButtonLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            BrushConverter bc = new BrushConverter();
            Brush brush = (Brush)bc.ConvertFrom("#41B1E1");
            UnderButtonLeft.Foreground = brush;
            UnderButtonLeft.Background = brush;
        }

        /// <summary>
        /// Highlightes UnderButtons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverButtonRight_MouseEnter(object sender, MouseEventArgs e)
        {
            BrushConverter bc = new BrushConverter();
            Brush brush = (Brush)bc.ConvertFrom("#41B1E1");
            UnderButtonRight.Foreground = brush;
            UnderButtonRight.Background = brush;
        }

        /// <summary>
        /// Makes the UnderButtons Return to normal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReturnButtonsToNumbers_MouseLeage(object sender, MouseEventArgs e)
        {
            BrushConverter bc = new BrushConverter();
            Brush brush = (Brush)bc.ConvertFrom("Transparent");
            UnderButtonRight.Foreground = brush;
            UnderButtonLeft.Foreground = brush;
            UnderButtonRight.Background = brush;
            UnderButtonLeft.Background = brush;
        }

        /// <summary>
        /// A Simple Logger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            //Disregard PVPNetSpam
            if (e.Exception.Message.Contains("too small for an Int32") || e.Exception.Message.Contains("Constructor on type "))
                return;
            Log("A first chance exception was thrown", "EXCEPTION");
            Log(e.Exception.Message, "EXCEPTION");
            Log(e.Exception.StackTrace, "EXCEPTION");
        }

        /// <summary>
        /// A simple Log Writer
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="type"></param>
        public void Log(String lines, String type = "LOG")
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(System.IO.Path.Combine(ExecutingDirectory, "LegendaryClientPatcher.log"), true);
            file.WriteLine(string.Format("({0} {1}) [{2}]: {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), type, lines));
            file.Close();
        }
        public void LogTextBox(string s)
        {
            Logbox.Text += s + Environment.NewLine;
        }
    }
}
