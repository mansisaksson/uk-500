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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using uk_500.Database;

namespace uk_500
{
    public partial class EditPeople : UserControl
    {
        private List<PersonModel> People = new List<PersonModel>();

        public EditPeople()
        {
            InitializeComponent();
        }

        private async Task Search()
        {
            SearchButton.IsEnabled = false;
            ResultField.Items.Clear();
            People = await PeopleRepository.FindPeopleByName(FirstNameField.Text, LastNameField.Text, 20);
            foreach (var person in People)
            {
                var personLabel = new Label();
                personLabel.Tag = person.uid.ToString();
                personLabel.Content = $"{person.first_name} {person.last_name} ({person.city})";
                ResultField.Items.Add(personLabel);
            }
            SearchButton.IsEnabled = true;
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await Search();
        }

        private async void ResultField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedLabel = ResultField.SelectedItem as Label;
            if (selectedLabel != null)
            {
                EditPerson popup = new EditPerson(People.Find(x => x.uid == long.Parse((ResultField.SelectedItem as Label).Tag.ToString())));
                popup.ShowDialog();
                await Search();
            }
        }
    }
}
