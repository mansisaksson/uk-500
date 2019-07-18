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
using uk_500.Database;

namespace uk_500
{
    public partial class Stats : UserControl
    {
        public Stats()
        {
            InitializeComponent();
        }

        private async void GetStatsButton_Click(object sender, RoutedEventArgs e)
        {
            GetStatsButton.IsEnabled = false;
            StatsListBox.Items.Clear();
            string[] stats = await PeopleRepository.GetFunStats();
            foreach (var stat in stats)
            {
                var displayLabel = new Label();
                displayLabel.Content = stat;
                StatsListBox.Items.Add(displayLabel);
            }
            GetStatsButton.IsEnabled = true;
        }
    }
}
