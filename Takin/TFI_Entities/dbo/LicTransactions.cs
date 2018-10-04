using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.Entities.dbo
{
    public class LicTransactions : EntityBase
    {
        public virtual int ReceiptID { get; set; }
        public virtual string TransactionID { get; set; }
        public virtual DateTime TransactionDate { get; set; }
        public virtual int idCurrency { get; set; }
        public virtual double dSum { get; set; }
        public virtual string TransAnswer { get; set; }
        public virtual string PaypalReceiptID { get; set; }
        public virtual string PaypalUserID { get; set; }
    }
}
