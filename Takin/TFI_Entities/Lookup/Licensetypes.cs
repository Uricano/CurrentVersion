using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.Entities.Lookup
{
    public class Licensetypes 
    {
        public virtual int Idlicensetype { get; set; }
        public virtual string Strname { get; set; }
    }
}
