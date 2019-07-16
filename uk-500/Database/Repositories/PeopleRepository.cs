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

        public static async Task<List<PersonModel>> ListPeople()
        {
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                var output = await cnn.QueryAsync<PersonModel>("SELECT * FROM People");
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
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                Console.WriteLine($"Inserting {People.Count} people...");

                var valuesString = new Func<bool, string>((bool addAtSymbol) =>
                {
                    var PropertiesList = typeof(PersonModel).GetProperties().OrderBy(x => x.MetadataToken);

                    string buildingString = "(";
                    foreach (var name in PropertiesList.Select(x => x.Name))
                    {
                        if (name == "uid")
                            continue;
                        buildingString += (addAtSymbol ? "@" : "") + name + ", ";
                    }
                    buildingString = buildingString.TrimEnd(new char[] { ',', ' ' });
                    buildingString += ")";
                    return buildingString;
                });

                cnn.Open();
                var trans = cnn.BeginTransaction();
                await cnn.ExecuteAsync($"INSERT INTO People {valuesString(false)} VALUES {valuesString(true)}", People, transaction: trans);
                trans.Commit();

                Console.WriteLine("Done.");
            }
        }

        public static async Task<List<PersonLocation>> GetAllPeopleLocations()
        {
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                var sql = "SELECT People.uid, Postcodes.longitude, Postcodes.latitude, Postcodes.eastings, Postcodes.northings FROM People INNER JOIN Postcodes ON People.postal = Postcodes.postcode";
                var output = await cnn.QueryAsync<PersonLocation>(sql);
                return output.ToList();
            }
        }
    }
}
