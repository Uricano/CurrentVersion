using System;
using System.Text;
using System.Collections.Generic;


namespace TFI.Entities.Lookup {
    
    public class Selexchanges {
        public Selexchanges() { Markets = new List<SelStockExchange>(); }
        public virtual int idExchangePackage { get; set; }
        public virtual string strName { get; set; }
        public virtual string idCodes { get; set; }
        public virtual IList<SelStockExchange> Markets { get; set; }
    }
}
