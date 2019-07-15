using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using uk_500.Database.Repositories;

namespace uk_500.Database
{
    /* 
     * Helper classes for deserializing the HTTP response. 
     * There is probably a prettier way of doing this where I don't need these classes.
     */
    class PostcodeResultModel
    {
        public string query { get; set; }
        public PostcodeModel result { get; set; }
    }

    class PostcodesResponseModel
    {
        public int status { get; set; }
        public List<PostcodeResultModel> result { get; set; }
    }

    /* 
     * The idea here is to create a crawler that scrapes the postcode data and places it
     * in a seperate table. This data is not likley to change and multiple people might share the same postcode.
     */
    class PostcodesCrawler
    {
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

        static readonly HttpClient client = new HttpClient();

        private static async Task<List<PostcodeModel>> GetPostcodeData(string[] postcodes)
        {
            if (postcodes.Length > 0)
            {
                try
                {
                    Console.WriteLine("Pulling postcode information...");

                    var jObject = new
                    {
                        filter = "",
                        postcodes = postcodes // Accepts up to 100 postcodes.
                    };
                    var stringContent = new StringContent(new JavaScriptSerializer().Serialize(jObject), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("http://api.postcodes.io/postcodes", stringContent);

                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    PostcodesResponseModel responseObject = new JavaScriptSerializer().Deserialize<PostcodesResponseModel>(responseBody);
                    return responseObject.result.Where(x => x.result != null).Select(x => x.result).ToList();
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                    return new List<PostcodeModel>();
                }
            }
            else
            {
                return new List<PostcodeModel>();
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
             * TODO: This works quite well for 500ish but I doubt it would be a good idea not to
             * add a delay of some sort inbetween the different HTTP reqests
             */
            var postalList = await PostcodeRepository.GetNewPostcodes();
            var BufferedLists = SplitList(postalList, 100);
            var CrawlTasks = new List<Task<List<PostcodeModel>>>();
            foreach (var list in BufferedLists)
            {
                CrawlTasks.Add(GetPostcodeData(list.ToArray()));
            }

            var results = await Task.WhenAll(CrawlTasks);

            await PostcodeRepository.InsertPostcodes(results.SelectMany(x => x).ToList());
        }
    }
}
