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

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for PlayerItemReplay.xaml
    /// </summary>
    public partial class PlayerItemReplay : UserControl
    {
        public PlayerItemReplay()
        {
            InitializeComponent();
        }
        public List<object> getChildElements()
        { 
            List<object> elements = new List<object>();
            elements.Add(ChampionIcon);
            elements.Add(PlayerNameLabel);
            elements.Add(File);
            elements.Add(gameItem1);
            elements.Add(gameItem2);
            elements.Add(gameItem3);
            elements.Add(gameItem4);
            elements.Add(gameItem5);
            elements.Add(gameTrinket);
            elements.Add(KDA);
            return elements;
        }
    }
}
