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
    internal class WorldTile
    {
        public int Density;

        public double accumulatedLat;
        public double accumulatedLng;

        public double avgLat => accumulatedLat / Density;
        public double avgLng => accumulatedLng / Density;
    }

    public partial class Map : UserControl
    {
        private double Scale = 1.0;

        private int GridWidth => (int)Math.Floor(62.0 * Scale);
        private int GridHeight => (int)Math.Floor(100.0 * Scale);

        private Dictionary<(int x, int y), WorldTile> WorldTileTable = new Dictionary<(int x, int y), WorldTile>();
        private List<PersonLocation> People = new List<PersonLocation>();

        public Map()
        {
            InitializeComponent();
        }

        private static double Remap(double OldValue, double OldMin, double OldMax, double NewMin, double NewMax)
        {
            return (((OldValue - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin;
        }

        private (int x, int y) GetPersonTileIndex(PersonLocation person)
        {
            /* TODO: Hard-coded max/min longitude and latitude, this breaks if we get values outside of this range */
            return ((int)Math.Round(Remap(person.longitude, -8.0f, 2f, 0, GridWidth - 1)), 
                (int)Math.Round(Remap(person.latitude, 49.0, 61.0, GridHeight - 1, 0)));
        }

        public async Task RebuildMap()
        {
            if (!double.TryParse(ResolutionScale.Text, out Scale))
                Scale = 1;

            ClearMap();

            foreach (var person in People)
            {
                var index = GetPersonTileIndex(person);
                
                WorldTile tile;
                if (!WorldTileTable.TryGetValue(index, out tile))
                {
                    tile = new WorldTile();
                    WorldTileTable.Add(index, tile);
                }

                tile.Density += 1;
                tile.accumulatedLat += person.latitude;
                tile.accumulatedLng += person.longitude;
            }

            if (WorldTileTable.Count < 2)
                return;

            /* Semi-ugly mix-code for displaying density scale */
            {
                int MaxDensity = WorldTileTable.Max(x => x.Value.Density);
                int AvgDensity = (int)WorldTileTable.Average(x => x.Value.Density);
                int MedianDensity = WorldTileTable.Values.OrderBy(x => x.Density).ToList().ElementAt(WorldTileTable.Count / 2).Density;

                WriteableBitmap bitmap = new WriteableBitmap(GridWidth, GridHeight, 0, 0, PixelFormats.Bgra32, null);
                byte[,,] pixels = new byte[GridWidth, GridHeight, 4];

                foreach (var tile in WorldTileTable)
                {
                    Console.WriteLine($"X:{tile.Key.x}, Y:{tile.Key.y}");
                    pixels[tile.Key.x, tile.Key.y, 2] = (byte)Remap(tile.Value.Density, 0, (MedianDensity + AvgDensity + MaxDensity) / 3, 0, 255);
                    pixels[tile.Key.x, tile.Key.y, 3] = 255;
                }

                // Copy the data into a one-dimensional array.
                byte[] pixels1d = new byte[GridHeight * GridWidth * 4];
                int index = 0;
                for (int row = 0; row < GridHeight; row++)
                {
                    for (int col = 0; col < GridWidth; col++)
                    {
                        for (int i = 0; i < 4; i++)
                            pixels1d[index++] = pixels[col, row, i];
                    }
                }

                // Update writeable bitmap with the colorArray to the image.
                Int32Rect rect = new Int32Rect(0, 0, GridWidth, GridHeight);
                int stride = 4 * GridWidth;
                bitmap.WritePixels(rect, pixels1d, stride, 0);

                //Set the Image source.
                RenderImage.Source = bitmap;
            }
        }

        public void ClearMap()
        {
            RenderImage.Source = null;
            WorldTileTable.Clear();
        }

        public void AddPeople(List<PersonLocation> People)
        {
            this.People = this.People.Union(People).ToList();
            RebuildMap();
        }

        // Haversine distance stolen from: https://stackoverflow.com/questions/41621957/a-more-efficient-haversine-function
        private static double HaversineDistance(double lat1, double lat2, double lon1, double lon2)
        {
            const double r = 6371000; // meters
            var dlat = (lat2 - lat1) / 2;
            var dlon = (lon2 - lon1) / 2;

            var q = Math.Pow(Math.Sin(dlat), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(q), Math.Sqrt(1 - q));

            var d = r * c;
            return d / 1000;
        }

        public async Task FindLargestCluster(float Location)
        {
            foreach (var person in People)
            {
                
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RebuildMap();
        }
    }
}
