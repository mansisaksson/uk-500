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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ImportCSV_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Table (.csv)|*.csv"; // Filter files by extension

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                ImportCSV.IsEnabled = false;
                await PeopleRepository.ImportCSV(dlg.FileName);
                ImportCSV.IsEnabled = true;
            }
        }

        private async void CrawlPostcodes_Click(object sender, RoutedEventArgs e)
        {
            CrawlPostcodes.IsEnabled = false;
            await PostcodesCrawler.StartCrawler();
            CrawlPostcodes.IsEnabled = true;
        }
    }
}
