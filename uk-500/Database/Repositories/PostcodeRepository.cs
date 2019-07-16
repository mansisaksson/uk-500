using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace uk_500.Database.Repositories
{
    class PostcodeRepository
    {
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        public static async Task<List<PostcodeModel>> ListPostcodes()
        {
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                var output = await cnn.QueryAsync<PostcodeModel>("SELECT * FROM Postcodes", new DynamicParameters());
                return output.ToList();
            }
        }

        /*
         * TODO: There exists a bulk insert package that could be useful here
         * This is reasonably fast however so probably not needed, unless really slow on large data-sets.
         */
        public static async Task InsertPostcodes(List<PostcodeModel> Postcodes)
        {
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                Console.WriteLine($"Inserting {Postcodes.Count} postcodes...");

                var valuesString = new Func<bool, string>((bool addAtSymbol) =>
                {
                    var PropertiesList = typeof(PostcodeModel).GetProperties().OrderBy(x => x.MetadataToken);

                    string buildingString = "(";
                    foreach (var name in PropertiesList.Select(x => x.Name))
                    {
                        buildingString += (addAtSymbol ? "@" : "") + name + ", ";
                    }
                    buildingString = buildingString.TrimEnd(new char[] { ',', ' ' });
                    buildingString += ")";
                    return buildingString;
                });

                cnn.Open();
                var trans = cnn.BeginTransaction();
                await cnn.ExecuteAsync($"INSERT INTO Postcodes {valuesString(false)} VALUES {valuesString(true)}", Postcodes, transaction: trans);
                trans.Commit();

                Console.WriteLine("Done.");
            }
        }

        /*
         * Returns postcodes that does not exist in the postcodes table
         */
        public static async Task<List<string>> GetNewPostcodes()
        {
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                var output = await cnn.QueryAsync(
                    "SELECT DISTINCT postal " +
                    "FROM People " +
                    "WHERE postal NOT IN (SELECT postcode FROM Postcodes)"
                );

                return output.Select<dynamic, string>(x => x.postal).ToList();
            }
        }
    }
}
