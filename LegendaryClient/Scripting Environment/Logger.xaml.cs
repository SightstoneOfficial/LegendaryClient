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
using System.Windows.Shapes;

namespace LegendaryClient.Scripting_Environment
{
	/// <summary>
	/// Interaktionslogik für Logger.xaml
	/// </summary>
	public partial class Logger : Window
	{
		public Logger()
		{
			InitializeComponent();
			Visibility = Visibility.Collapsed;
			textBox.Text = "";
			Closing += Logger_Closing;
		}

		public void Clear()
		{
			textBox.Text = "";
		}

		private void Logger_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			Visibility = Visibility.Collapsed;
		}


		public void Log(string text)
		{
			textBox.Text = textBox.Text + text + Environment.NewLine;
		}
	}
}
