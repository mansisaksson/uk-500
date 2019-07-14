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
        string postcode;
        int quality;
        int eastings;
        int northings;
        string country;
        string nhs_ha;
        double longitude;
        double latitude;
        string european_electoral_region;
        string primary_care_trust;
        string region;
        string lsoa;
        string msoa;
        string incode;
        string outcode;
        string parliamentary_constituency;
        string admin_district;
        string parish;
        string admin_county;
        string admin_ward;
        string ced;
        string ccg;
        string nuts;
        //PostcodeCodesModel codes;
    }
}
