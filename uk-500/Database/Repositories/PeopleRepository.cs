using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uk_500.Database.Repositories;
using Dapper;
using Microsoft.VisualBasic.FileIO;
using System.Reflection;

namespace uk_500.Database
{
    class PeopleRepository
    {
        private static string Table = "Person";
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

        public static void ImportCSV(string filepath)
        {
            using (TextFieldParser parser = new TextFieldParser(filepath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                string[] PropertyNames = parser.ReadFields();

                List<PersonModel> people = new List<PersonModel>();
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields.Length > 0)
                    {
                        PersonModel person = new PersonModel();
                        for (int i = 0; i < fields.Length; i++)
                        {
                            FieldInfo field = typeof(PersonModel).GetField(PropertyNames[i]);
                            if (field != null)
                            {
                                field.SetValue(person, fields[i]);
                            }
                        }
                        people.Add(person);
                    }
                }
            }

            // TODO: Insert into Database
        }

        public static async Task<List<PersonModel>> ListEntries()
        {
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                var output = cnn.Query<PersonModel>($"SELECT * FROM ${Table}", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task SavePerson(PersonModel Person)
        {
            // TODO: Decide on how to insert people into the db

            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                var output = cnn.Query<PersonModel>($"SELECT * FROM ${Table} WHERE uid=@uid", Person);
                if (output.ToList().Count > 0)
                {
                    
                }
                else
                {
                    string query = $"INSERT INTO {Table} (" +
                        "first_name, " +
                        "last_name, " +
                        "company_name, " +
                        "address, " +
                        "postal, " +
                        "phone1, " +
                        "phone2, " +
                        "email, " +
                        "web" +
                    ")";

                    cnn.Execute(query, Person);
                }
            }
        }
    }
}
