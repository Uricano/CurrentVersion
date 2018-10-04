using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.Entities.Lookup
{
    public class SelCurrency
    {
        public virtual string CurrencyID { get; set; }
        public virtual string Name { get; set; }
        public virtual string Symbol { get; set; }
        public virtual string Sign { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual Int16 rank { get; set; }
    }
}
