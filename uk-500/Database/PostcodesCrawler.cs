using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uk_500.Database
{
    /* 
     * The idea here is to create a crawler that scapes the postcode data and place it
     * in a seperate table as its contents will never change. Multiple people might also
     * share the same postcode.
     */
    class PostcodesCrawler
    {
        void StartCrawler()
        {
            // TODO: 
            // 1. Get all postcodes from people that does not have an entry in geocode table
            // 2. Scrape the postcode info from https://api.postcodes.io/postcodes
            // 3. Put it into the PostcodesRepo
        }
    }
}
