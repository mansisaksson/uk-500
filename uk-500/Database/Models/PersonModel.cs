using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uk_500.Database
{
    public class PersonModel
    {
        public int uid = -1;
        public string first_name;
        public string last_name;
        public string company_name;
        public string address;
        //public string city; I do not need these as I've got the postcode, it's also something I don't want the user to edit
        //public string county;
        public string postal;
        public string phone1;
        public string phone2;
        public string email;
        public string web;
    }
}
