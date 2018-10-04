using System;
using System.Text;
using System.Collections.Generic;
using TFI.Entities.Lookup;

namespace TFI.Entities.dbo
{
    
    public class Userlicenses {
        public virtual int LicenseID { get; set; }
        public virtual Licservices tb_LicServices { get; set; }
        public virtual Users User { get; set; }
        public virtual Licensetypes Licensetypes { get; set; }
        public virtual LicTransactions Transaction { get; set; }
        public virtual long? UserID { get; set; }
        public virtual DateTime? dtExpirationDate { get; set; }
        public virtual DateTime? dtActivationDate { get; set; }
        public virtual DateTime? dtPurchaseDate { get; set; }
        public virtual bool isTrial { get; set; }
        public virtual IList<Licenseexchanges> Licenseexchanges { get; set; }
    }
}
