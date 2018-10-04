using Cherries.Models.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.dbo
{
    public class LicenceService
    {
        public int Idlicservice { get; set; }
        public string StrServiceType { get; set; }
        public string Strservicename { get; set; }
        public int Imonths { get; set; }
        public int Iportfolios { get; set; }
        public double Dstartprice { get; set; }
        public double Dnewexchangeprice { get; set; }
        public int Ibaseexchanges { get; set; }
        public bool IsTrial { get; set; }

        public LicenceService()
        {
           
        }
    }
}
