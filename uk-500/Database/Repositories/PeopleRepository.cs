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
using System.Windows;

namespace uk_500.Database
{
    class PeopleRepository
    {
        private static string ConnectionString => ConfigurationManager.ConnectionStrings[(string)Application.Current.Properties["database"]].ConnectionString;

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

        public static async Task<string[]> GetFunStats()
        {
            List<Task<string>> statTasks = new List<Task<string>>();

            // Get most common e-mail
            statTasks.Add(Task.Run(() =>
            {
                using (var cnn = new SQLiteConnection(ConnectionString))
                {
                    var sql = @"SELECT substr(email, instr(email, '@') + 1) AS Domain, count(email) DomainCount
                                FROM People
                                WHERE length(email) > 0
                                GROUP BY substr(email, instr(email, '@') + 1)
                                ORDER BY DomainCount DESC
                                LIMIT 3";
                    var result = cnn.ExecuteReader(sql);
                    string output = "The most common e-mail addresses in the UK are:\n";
                    int count = 1;
                    while (result.Read())
                    {
                        output += $"Nr {count}: {result["Domain"]}, Count {result["DomainCount"]}\n";
                        count++;
                    }
                    return output;
                }
            }));


            // Get most common names
            statTasks.Add(Task.Run(() =>
            {
                using (var cnn = new SQLiteConnection(ConnectionString))
                {
                    var sql = @"SELECT first_name, count(first_name) NameCount
                                FROM People
                                WHERE length(first_name) > 0
                                GROUP BY first_name
                                ORDER BY NameCount DESC
                                LIMIT 1";
                    var result = cnn.ExecuteReader(sql);
                    string firstNameStr = "";
                    if (result.Read())
                        firstNameStr = $"The most common name in the UK is {result["first_name"]}\n";

                    sql = @"SELECT last_name, count(last_name) NameCount
                                FROM People
                                WHERE length(last_name) > 0
                                GROUP BY last_name
                                ORDER BY NameCount DESC
                                LIMIT 1";
                    result = cnn.ExecuteReader(sql);
                    string lastNameStr = "";
                    if (result.Read())
                        lastNameStr = $"The most common surname in the UK is {result["last_name"]}\n";

                    return firstNameStr + lastNameStr;
                }
            }));

            return await Task.WhenAll(statTasks);
        }
    }
}
