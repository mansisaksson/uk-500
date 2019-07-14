using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Web.Script.Serialization;

namespace uk_500.Database
{
    /* 
     * The idea here is to create a crawler that scrapes the postcode data and places it
     * in a seperate table. This data is not likley to change and multiple people might share the same postcode.
     */
    class PostcodesCrawler
    {
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

        static readonly HttpClient client = new HttpClient();

        private static async Task GetPostcodeData(string[] postcodes)
        {
            try
            {
                var jObject = new
                {
                    filter = "",
                    postcodes = postcodes // Accepts up to 100 postcodes.
                };
                var stringContent = new StringContent(new JavaScriptSerializer().Serialize(jObject), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("http://api.postcodes.io/postcodes", stringContent);
                
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        public static async Task StartCrawler()
        {
            /*
             * TODO: This works qute well for 500ish but I doubt it would be a good idea to not to
             * add a delay of some sort inbetween the different HTTP requiests
             */
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                var output = await cnn.QueryAsync(
                    "SELECT postal " +
                    "FROM People " +
                    "WHERE postal NOT IN (SELECT postcode FROM Postcodes)"
                );

                var postalList = output.ToList().Select<dynamic, string>(x => x.postal).ToList();
                var BufferedLists = SplitList(postalList, 100);
                var CrawlTasks = new List<Task>();
                foreach (var list in BufferedLists)
                {
                    CrawlTasks.Add(GetPostcodeData(list.ToArray()));
                }

                await Task.WhenAll(CrawlTasks);

                Console.WriteLine("Done");
            }
        }
    }
}
