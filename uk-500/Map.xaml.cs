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

        public Rectangle RenderTile;
    }

    public partial class Map : UserControl
    {
        private static int GridWidth = 62;
        private static int GridHeight = 100;

        private Dictionary<long, WorldTile> WorldTileTable = new Dictionary<long, WorldTile>();
        private List<PersonLocation> People = new List<PersonLocation>();

        private double Scale = 1.0;
        private static double RectSize = 5.0;

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
            return ((int)Math.Round(Remap(person.longitude, -7f, 2f, 0, (GridWidth - 1) * Scale)), 
                (int)Math.Round(Remap(person.latitude, 50.0, 60.0, (GridHeight - 1) * Scale, 0)));
        }

        public void RebuildMap(double resolutionScale)
        {
            Scale = resolutionScale;

            ClearMap();

            foreach (var person in People)
            {
                var index = GetPersonTileIndex(person);

                long hashKey = (long)index.x << 32 | index.y & 0xFFFFFFFFL;
                WorldTile tile;
                if (!WorldTileTable.TryGetValue(hashKey, out tile))
                {
                    tile = new WorldTile();
                    WorldTileTable.Add(hashKey, tile);
                }

                if (tile.RenderTile == null)
                {
                    tile.RenderTile = new Rectangle();
                    tile.RenderTile.Height = RectSize / Scale;
                    tile.RenderTile.Width = RectSize / Scale;
                    tile.RenderTile.Fill = new SolidColorBrush(Colors.Red);
                    
                    Canvas.SetLeft(tile.RenderTile, 100 + index.x * RectSize / Scale);
                    Canvas.SetTop(tile.RenderTile, 75 + index.y * RectSize / Scale);

                    MapCanvas.Children.Add(tile.RenderTile);
                }

                tile.Density += 1;
                tile.accumulatedLat += person.latitude;
                tile.accumulatedLng += person.longitude;
            }

            int MaxDensity = WorldTileTable.Max(x => x.Value.Density);

            foreach (var tile in WorldTileTable)
            {
                var PointColor = new Color();
                PointColor.R = (byte)Remap(tile.Value.Density, 0, MaxDensity, 0, 255);
                PointColor.G = 0;
                PointColor.B = 0;
                PointColor.A = 255;
                tile.Value.RenderTile.Fill = new SolidColorBrush(PointColor);
            }
        }

        public void ClearMap()
        {
            foreach (var worldTilePair in WorldTileTable)
            {
                if (worldTilePair.Value.RenderTile != null)
                    MapCanvas.Children.Remove(worldTilePair.Value.RenderTile);
            }

            WorldTileTable.Clear();
        }

        public void AddPeople(List<PersonLocation> People)
        {
            this.People = this.People.Union(People).ToList();

            double resScale = 100;
            if (!double.TryParse(ResolutionScale.Text, out resScale))
                resScale = 100;

            RebuildMap(Math.Min(Math.Max(resScale, 0.1), 10));
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
    }
}
