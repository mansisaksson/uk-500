using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uk_500.Database
{
    public class PersonModel
    {
        public int uid { get; set; } = -1;
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company_name { get; set; }
        public string address { get; set; }
        //public string city { get; set; }; I do not need these as I've got the postcode, it's also something I don't want the user to edit
        //public string county { get; set; };
        public string postal { get; set; }
        public string phone1 { get; set; }
        public string phone2 { get; set; }
        public string email { get; set; }
        public string web { get; set; }
    }
}
