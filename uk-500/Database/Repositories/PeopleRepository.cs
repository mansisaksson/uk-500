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

        public static async Task InsertPeople(List<PersonModel> People)
        {
            // TODO: Really inefficient way of adding people to the database, look if there's any "bulk add" support in Dapper
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                Console.WriteLine("Ingesting data into database...");

                await cnn.ExecuteAsync("INSERT INTO People " +
                    "(first_name, last_name, company_name, address, postal, phone1, phone2, email, web)" +
                    "VALUES(@first_name, @last_name, @company_name, @address, @postal, @phone1, @phone2, @email, @web)",
                    People
                );

                Console.WriteLine("Done");
            }
        }
    }
}
