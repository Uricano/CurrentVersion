using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.Entities.Lookup
{
    public class Country
    {
        public virtual int CountryID { get; set; }
        public virtual string CountryCode { get; set; }
        public virtual string CountryName { get; set; }
    }
}
