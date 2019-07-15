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
using uk_500.Database.Repositories;

namespace uk_500
{
    public partial class Map : UserControl
    {
        private Dictionary<int, Ellipse> PointIdTable = new Dictionary<int, Ellipse>();


        public Map()
        {
            InitializeComponent();
        }

        public async Task RebuildMap()
        {
            ClearMap();

            var postcodes = await PostcodeRepository.ListPostcodes();
            foreach (var postcode in postcodes)
            {
                int pointSize = 5;
                float scale = 0.0005f;

                Ellipse point = new Ellipse();
                point.Height = pointSize;
                point.Width = pointSize;
                point.Fill = new SolidColorBrush(Colors.Red);

                Canvas.SetRight(point, postcode.eastings * scale);
                Canvas.SetBottom(point, (postcode.northings) * scale);

                MapCanvas.Children.Add(point);
            }
        }

        public void ClearMap()
        {
            foreach (var idPointPair in PointIdTable)
            {
                if (idPointPair.Value != null)
                    MapCanvas.Children.Remove(idPointPair.Value);
            }

            PointIdTable.Clear();
        }
    }
}
