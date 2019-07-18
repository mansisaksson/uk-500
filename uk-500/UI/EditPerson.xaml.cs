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
using System.Windows.Shapes;
using uk_500.Database;

namespace uk_500
{
    public partial class EditPerson : Window
    {
        private PersonModel Person;
        public EditPerson(PersonModel person)
        {
            InitializeComponent();

            Person = person;

            Title = $"Edit {person.first_name} {person.last_name}";

            var PropertiesList = typeof(PersonModel).GetProperties().OrderBy(x => x.MetadataToken);

            int count = 0;
            foreach (var property in PropertiesList)
            {
                if (property.Name == "uid")
                    continue;

                var label = new Label();
                label.Name = property.Name + "_label";
                label.Content = property.Name;
                Canvas.SetLeft(label, 10);
                Canvas.SetTop(label, 20.0 * count);
                MainCanvas.Children.Add(label);

                var textBox = new TextBox();
                textBox.Name = property.Name + "_textBox";
                textBox.Text = (string)property.GetValue(person);
                Canvas.SetLeft(textBox, 100);
                Canvas.SetTop(textBox, 20.0 * count);
                MainCanvas.Children.Add(textBox);

                count++;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var canvasChild in MainCanvas.Children)
            {
                var textBox = canvasChild as TextBox;
                if (textBox != null)
                {
                    var PropertiesList = typeof(PersonModel).GetProperties().ToList();
                    var property = PropertiesList.Find(x => x.Name + "_textBox" == textBox.Name);
                    property.SetValue(Person, textBox.Text);
                }
            }

            await PeopleRepository.UpdatePeople(new List<PersonModel>{ Person });
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
