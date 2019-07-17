using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uk_500.Database
{
    public class PersonLocation : IEquatable<PersonLocation>
    {
        public long uid { get; set; }
        public long eastings { get; set; }
        public long northings { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }

        public bool Equals(PersonLocation other)
        {
            return uid.Equals(other);
        }

        public override int GetHashCode()
        {
            return uid.GetHashCode();
        }
    }

    public class PersonModel
    {
        public long uid { get; set; } = -1;
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company_name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string postal { get; set; }
        public string phone1 { get; set; }
        public string phone2 { get; set; }
        public string email { get; set; }
        public string web { get; set; }
    }
}
