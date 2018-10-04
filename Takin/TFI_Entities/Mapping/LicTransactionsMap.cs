using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.dbo;

namespace TFI.Entities.Mapping
{
    public class LicTransactionsMap : ClassMap<LicTransactions>
    {
        public LicTransactionsMap()
        {
            Table("tblLic_LicTransactions");
            //LazyLoad();
            Id(x => x.ReceiptID).GeneratedBy.Identity().Column("ReceiptID");
            Map(x => x.TransactionID).Column("TransactionID");
            Map(x => x.TransactionDate).Column("TransactionDate");
            Map(x => x.idCurrency).Column("idCurrency");
            Map(x => x.dSum).Column("dSum");
            Map(x => x.TransAnswer).Column("TransAnswer");
            Map(x => x.PaypalReceiptID).Column("PaypalReceiptID");
            Map(x => x.PaypalUserID).Column("PaypalUserID");
        }
    }
}
