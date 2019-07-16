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
            await Task.Run(async () => // Might be a bit unnecessary to do this async?
            {
                using (var cnn = new SQLiteConnection(ConnectionString))
                {
                    var PropertiesList = typeof(PostcodeModel).GetProperties().OrderBy(x => x.MetadataToken);
                    var BufferedLists = SplitList(Postcodes, 999 / PropertiesList.Count()); // There is a limit of about 999 variables at a time
                    foreach (var chunk in BufferedLists)
                    {
                        Console.WriteLine($"Inserting {chunk.Count} postcodes...");

                        var valuesString = new Func<int, string>((int index) =>
                        {
                            string buildingString = "(";
                            foreach (var name in PropertiesList.Select(x => x.Name))
                            {
                                buildingString += "@" + name + "_" + index + ", ";
                            }
                            buildingString = buildingString.TrimEnd(new char[] { ',', ' ' });
                            buildingString += ")";
                            return buildingString;
                        });

                        var ParamsObject = new Func<ExpandoObject>(() =>
                        {
                            var exo = new ExpandoObject();
                            for (int i = 0; i < chunk.Count; i++)
                            {
                                foreach (var property in PropertiesList)
                                {
                                    ((IDictionary<string, object>)exo).Add(property.Name + "_" + i, property.GetValue(chunk[i]));
                                }
                            }
                            return exo;
                        })();

                        string sql = "INSERT INTO Postcodes " + "VALUES ";
                        for (int i = 0; i < chunk.Count; i++)
                        {
                            sql += valuesString(i);
                            sql += chunk.Count - 1 == i ? "" : ",\n";
                        }

                        await cnn.ExecuteAsync(sql, ParamsObject);
                    }
                }
            });
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
