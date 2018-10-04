using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.dbo;

namespace TFI.Entities.Lookup
{
    public class Licenseexchanges
    {
        public virtual int Idlicexchanges { get; set; }
        public virtual Userlicenses Userlicenses { get; set; }
        public virtual SelStockExchange Stockexchanges { get; set; }
    }
}
