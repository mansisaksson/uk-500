using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.VisualBasic.FileIO;
using System.Reflection;
using System.Dynamic;

namespace uk_500.Database
{
    class PeopleRepository
    {
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

        public async static Task ImportCSV(string filepath)
        {
            List<PersonModel> people = await Task.Run(() =>
            {
                List<PersonModel> loadedPeople = new List<PersonModel>();
                using (TextFieldParser parser = new TextFieldParser(filepath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    string[] PropertyNames = parser.ReadFields();

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        if (fields.Length > 0)
                        {
                            PersonModel person = new PersonModel();
                            for (int i = 0; i < fields.Length; i++)
                            {
                                var propertyInfo = typeof(PersonModel).GetProperty(PropertyNames[i]);
                                if (propertyInfo != null)
                                {
                                    propertyInfo.SetValue(person, fields[i]);
                                }
                            }
                            loadedPeople.Add(person);
                        }
                    }
                }
                return loadedPeople;
            });
            
            if (people.Count > 0)
                await InsertPeople(people);
        }

        public static async Task<List<PersonModel>> ListEntries()
        {
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                var output = await cnn.QueryAsync<PersonModel>("SELECT * FROM People", new DynamicParameters());
                return output.ToList();
            }
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        /*
         * TODO: There exists a bulk insert package that could be useful here
         * This is reasonably fast however so probably not needed, unless really slow on large data-sets.
         */
        public static async Task InsertPeople(List<PersonModel> People)
        {
            await Task.Run(async () => // Might be a bit unnecessary to do this async?
            {
                using (var cnn = new SQLiteConnection(ConnectionString))
                {
                    var PropertiesList = typeof(PersonModel).GetProperties().OrderBy(x => x.MetadataToken);
                    var BufferedLists = SplitList(People, 999 / PropertiesList.Count()); // There is a limit of 999 variables at a time
                    foreach (var chunk in BufferedLists)
                    {
                        Console.WriteLine($"Inserting {chunk.Count} people...");

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
                                    var value = property.Name == "uid" ? null : property.GetValue(chunk[i]); // Auto-set by the DB
                                    ((IDictionary<string, object>)exo).Add(property.Name + "_" + i, value);
                                }
                            }
                            return exo;
                        })();

                        string sql = "INSERT INTO People " + "VALUES ";
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
    }
}
