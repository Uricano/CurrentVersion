using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.dbo;

namespace TFI.Entities.Mapping
{
    class PriceReturnMap : ClassMap<PriceReturn>
    {

        public PriceReturnMap()
        {
            Table("tbl_PriceReturns");
            ReadOnly();
            CompositeId().KeyProperty(x => x.idSecurity, "idSecurity")
                         .KeyProperty(x => x.idCurrency, "Currency")    //LR: where do we use Currency, not idCurrency?
                         .KeyProperty(x => x.dtDate, "dDate");

            Map(x => x.dtDate).Column("dDate").Not.Nullable();          //LR: where do we use dDate, not dtDate
            Map(x => x.dReturn).Column("dReturn");
            Map(x => x.fAdjClose).Column("fAdjClose");
            Map(x => x.fAdjClosePrevWeek).Column("fAdjClosePrevWeek");

        }//Constructor


    }//of Entity Map
}
