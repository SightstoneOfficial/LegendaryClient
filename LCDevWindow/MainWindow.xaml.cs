using LCDevWindow.Commands;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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

namespace LCDevWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Main.win = this;
            Log("LegendaryClient Logger. Starting Pipe, please wait.", Brushes.DarkRed);
        }
        public void Log(string text, SolidColorBrush color)
        {
            var tr = new TextRange(LogWindow.Document.ContentEnd, LogWindow.Document.ContentEnd);
            tr.Text = text + Environment.NewLine;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            LogWindow.ScrollToEnd();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string m = DevCommand.Text;
            if (m.Contains("(") && m.Contains(")"))
            {
                DevCommand.Text = "";
                string[] tempsplit = m.Split('(');

                Command x = Command.GetCommand(tempsplit[0]);
                if (x != null)
                {
                    List<String> splittwo = new List<String>();

                    string[] xm = tempsplit[1].Replace(")", "").Split(',');
                    foreach (string xd in xm)
                        splittwo.Add(xd);
                    x.ActivateCommand(splittwo.ToArray());
                }
                else
                {
                    Log("Invalid Command! Check out the help tips by doing \"Help()\" (Capitals Matter!!!)", Brushes.Red);
                }
            }
            else
            {
                Log("Invalid Command! Check out the help tips by doing \"Help()\" (Capitals Matter!!!)", Brushes.Red);
            }
        }
    }
}
