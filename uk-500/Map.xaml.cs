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
        private int GridWidth => (int)Math.Floor(62.0 * Scale);
        private int GridHeight => (int)Math.Floor(100.0 * Scale);

        private Dictionary<(int x, int y), WorldTile> WorldTileTable = new Dictionary<(int x, int y), WorldTile>();
        private List<PersonLocation> People = new List<PersonLocation>();

        private double Scale = 1.0;
        private (double Min, double Max) LngBounds = (0, 0);
        private (double Min, double Max) LatBounds = (0, 0);

        public Map()
        {
            InitializeComponent();
        }

        private static double Remap(double OldValue, double OldMin, double OldMax, double NewMin, double NewMax)
        {
            return (((OldValue - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin;
        }

        private (int x, int y) GetLngLatTileIndex((double lng, double lat) lngLat)
        {
            return ((int)Math.Round(Remap(lngLat.lng, LngBounds.Min, LngBounds.Max, 0, GridWidth - 1)), 
                    (int)Math.Round(Remap(lngLat.lat, LatBounds.Min, LatBounds.Max, GridHeight - 1, 0)));
        }

        private (double lng, double lat) GetTileLngLat((int x, int y) index)
        {
            return (Remap(index.x, 0, GridWidth - 1, LngBounds.Min, LngBounds.Max),
                    Remap(index.y, 0, GridHeight - 1, LatBounds.Min, LatBounds.Max));
        }

        public void RebuildMap()
        {
            ClearMap();

            if (People.Count < 2)
                return;

            // Update member variables
            {
                Scale = ResolutionSlider.Value / 100;

                LngBounds.Min = People.Min(x => x.longitude);
                LngBounds.Max = People.Max(x => x.longitude);
                LatBounds.Min = People.Min(x => x.latitude);
                LatBounds.Max = People.Max(x => x.latitude);
            }

            foreach (var person in People)
            {
                var index = GetLngLatTileIndex((person.longitude, person.latitude));
                
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

                Int32Rect rect = new Int32Rect(0, 0, GridWidth, GridHeight);
                int stride = 4 * GridWidth;
                bitmap.WritePixels(rect, pixels1d, stride, 0);

                RenderImage.Source = bitmap;
            }
        }

        public void ClearMap()
        {
            LargestAreaSphere.Visibility = Visibility.Hidden;
            RenderImage.Source = null;
            WorldTileTable.Clear();
        }

        public void SetPeople(List<PersonLocation> PeopleLocations)
        {
            //this.People = this.People.Union(People).ToList();
            People = PeopleLocations;
            RebuildMap();
        }

        // Haversine distance stolen from: https://stackoverflow.com/questions/27928/calculate-distance-between-two-latitude-longitude-points-haversine-formula
        private static double HaversineDistance((double lon, double lat) A, (double lon, double lat) B)
        {
            var deg2rad = new Func<double, double>((double deg) =>  { return deg * (Math.PI / 180); });

            var R = 6371; // Radius of the earth in km
            var dLat = deg2rad(B.lat - A.lat);  // deg2rad below
            var dLon = deg2rad(B.lon - A.lon);
            var a =
              Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(deg2rad(A.lat)) * Math.Cos(deg2rad(B.lat)) *
              Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
              ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        (double longitude, double latitude) FindLargestAreaInRadius(double radius)
        {
            int LargestDensity = 0;
            (double x, double y) LargestDensityLngLat = (0, 0);

            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    (double x, double y) tileCenter = new Func<(double x, double y)>(() =>
                    {
                        if (WorldTileTable.ContainsKey((x, y)))
                            return (WorldTileTable[(x, y)].avgLng, WorldTileTable[(x, y)].avgLat);

                        return GetTileLngLat((x, y));
                    })();

                    // TODO: We do a square search around the center which causes us to check a few irrelevant tiles
                    // might be more optimal to search in a circular pattern.
                    (int x, int y)[] searchDir = new (int x, int y)[]
                    {
                        (-1, 1),
                        (0, 1),
                        (1, 1),
                        (-1, 0),
                        (0, 1),
                        (-1, -1),
                        (0, -1),
                        (1, -1)
                    };

                    int accumulatedDensity = 0;
                    int count = 1;
                    while (true)
                    {
                        bool isInsideRadius = false;
                        foreach (var dir in searchDir)
                        {
                            (int x, int y) index = (x + dir.x * count, y + dir.y * count);
                            if (WorldTileTable.ContainsKey(index))
                            {
                                var adjacentTile = WorldTileTable[(index.x, index.y)];
                                (double x, double y) longLat = (adjacentTile.avgLng, adjacentTile.avgLat);
                                if (HaversineDistance(tileCenter, longLat) <= radius)
                                {
                                    accumulatedDensity += adjacentTile.Density;
                                    isInsideRadius = true;
                                }
                            }
                        }

                        if (!isInsideRadius)
                            break;

                        count++;
                    }

                    if (accumulatedDensity > LargestDensity)
                    {
                        LargestDensity = accumulatedDensity;
                        LargestDensityLngLat = tileCenter;
                    }
                }
            }

            return LargestDensityLngLat;
        }


        private void ApplyResButton_Click(object sender, RoutedEventArgs e)
        {
            RebuildMap();
        }

        private async void FindLargestArea_Click(object sender, RoutedEventArgs e)
        {
            double Radius = AreaRadiusSlider.Value;

            FindLargestArea.IsEnabled = false;
            var LargestAreaLocation = await Task.Run(() => { return FindLargestAreaInRadius(Radius); });
            FindLargestArea.IsEnabled = true;

            LargestAreaSphere.Visibility = Visibility.Visible;
            LargestAreaSphere.Width = Radius;
            LargestAreaSphere.Height = Radius;

            var gridIndex = GetLngLatTileIndex(LargestAreaLocation);
            Canvas.SetLeft(LargestAreaSphere, ((float)gridIndex.x / GridWidth) * RenderImage.ActualWidth - LargestAreaSphere.Width / 2.0);
            Canvas.SetTop(LargestAreaSphere, ((float)gridIndex.y / GridHeight) * RenderImage.ActualHeight - LargestAreaSphere.Height / 2.0);
        }

        private void ResolutionSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            RebuildMap();
        }
    }
}
