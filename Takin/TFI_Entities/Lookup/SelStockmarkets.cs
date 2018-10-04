using System;
using System.Text;
using System.Collections.Generic;
using TFI.Entities.Lookup;

namespace TFI.Entities.Lookup {
    
    public class SelStockExchange : LookupBase
    {
        public SelStockExchange() { }
        public virtual SelCurrency Currency { get; set; }
        public virtual DateTime LastUpdate { get; set; }
        
    }
}
