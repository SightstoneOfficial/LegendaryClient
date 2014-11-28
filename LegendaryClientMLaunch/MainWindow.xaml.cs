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

namespace LegendaryClientMLaunch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// Used to show options that users can use (replays without login)
    /// * Also allows users to change key bindings, sound volume + more without login
    /// * Alow Garnea Users to use LegendaryClient
    /// * Designed not to be in standard installs (Little UI thought, UNORGANIZED CODE). 
    /// * Only certain users should need this (Garnea = Required).
    /// ONLY IN ADVANCED INSTALLS. DO NOT ATTACH IN NORMAL BINARY
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
