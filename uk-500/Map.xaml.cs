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
        private Dictionary<long, Shape> PointIdTable = new Dictionary<long, Shape>();

        private static int GrixWidth = 50;
        private static int GrixHeight = 80;
        private int[,] DensityGrid = new int[GrixWidth, GrixHeight];

        public Map()
        {
            InitializeComponent();
        }

        private static int Remap(int OldValue, int OldMin, int OldMax, int NewMax, int NewMin)
        {
            return (((OldValue - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin;
        }

        public async Task RebuildMap()
        {
            ClearMap();

            var postcodes = await PostcodeRepository.ListPostcodes();
            int[] Min = new int[2] { int.MaxValue, int.MaxValue };
            int[] Max = new int[2] { int.MinValue, int.MinValue };
            foreach (var postcode in postcodes)
            {
                Min[0] = Math.Min(postcode.eastings, Min[0]);
                Min[1] = Math.Min(postcode.northings, Min[1]);
                Max[0] = Math.Max(postcode.eastings, Max[0]);
                Max[1] = Math.Max(postcode.northings, Max[1]);
            }

            foreach (var postcode in postcodes)
            {
                int[] index = new int[2] {
                    Remap(postcode.eastings, Min[0], Max[0], 0, GrixWidth - 1),
                    Remap(postcode.northings, Min[1], Max[1], 0, GrixHeight - 1)
                };

                DensityGrid[index[0], index[1]]++;

                long hashKey = (long)index[0] << 32 | index[1] & 0xFFFFFFFFL;
                if (!PointIdTable.ContainsKey((long)index[0] << 32 | index[1] & 0xFFFFFFFFL))
                {
                    int rectSize = 7;

                    Rectangle rect = new Rectangle();
                    rect.Height = rectSize;
                    rect.Width = rectSize;
                    rect.Fill = new SolidColorBrush(Colors.Red);

                    Canvas.SetLeft(rect, 20 + index[0] * rectSize);
                    Canvas.SetTop(rect, 20 + index[1] * rectSize);

                    MapCanvas.Children.Add(rect);
                    PointIdTable.Add(hashKey, rect);
                }

                var PointColor = new Color();
                PointColor.R = (byte)Math.Min(DensityGrid[index[0], index[1]] * 50, 255);
                PointColor.G = 0;
                PointColor.B = 0;
                PointColor.A = 255;
                PointIdTable[hashKey].Fill = new SolidColorBrush(PointColor);
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
