using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uk_500.Database
{
    // This information is not really interesting at the moment and 
    // adds some unnecessary complexity (would need an additional table in the Database).
    //class PostcodeCodesModel
    //{
    //    string admin_district;
    //    string admin_county;
    //    string admin_ward;
    //    string parish;
    //    string parliamentary_constituency;
    //    string ccg;
    //    string ced;
    //    string nuts;
    //}

    class PostcodeModel
    {
        public string postcode { get; set; }
        public long quality { get; set; }
        public long eastings { get; set; }
        public long northings { get; set; }
        public string country { get; set; }
        public string nhs_ha { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string european_electoral_region { get; set; }
        public string primary_care_trust { get; set; }
        public string region { get; set; }
        public string lsoa { get; set; }
        public string msoa { get; set; }
        public string incode { get; set; }
        public string outcode { get; set; }
        public string parliamentary_constituency { get; set; }
        public string admin_district { get; set; }
        public string parish { get; set; }
        public string admin_county { get; set; }
        public string admin_ward { get; set; }
        public string ced { get; set; }
        public string ccg { get; set; }
        public string nuts { get; set; }
        //PostcodeCodesModel codes;
    }
}
