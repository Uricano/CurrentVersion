using System;
using System.Text;
using System.Collections.Generic;
using TFI.Entities.Lookup;

namespace TFI.Entities.dbo
{
    
    public class Licservices
    {
        public Licservices() { }
        public virtual int Idlicservice { get; set; }
        public virtual string StrServiceType { get; set; }
        public virtual string Strservicename { get; set; }
        public virtual int Imonths { get; set; }
        public virtual int Iportfolios { get; set; }
        public virtual double Dstartprice { get; set; }
        public virtual double Dnewexchangeprice { get; set; }
        public virtual double Ibaseexchanges { get; set; }
        public virtual bool IsTrial { get; set; }
    }
}
