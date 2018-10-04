using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.Entities.dbo;

namespace TFI.Entities.Mapping
{
    public class PriceMap : ClassMap<Price>
    {

        public PriceMap()
        {
            Table("v_Prices");
            ReadOnly();
            //LazyLoad();
            CompositeId().KeyProperty(x => x.idSecurity, "idSecurity")
                         .KeyProperty(x => x.dDate, "dDate");
           
            //References(x => x.Securities).Column("Idsecurity");
            Map(x => x.dDate).Column("dDate").Not.Nullable();
            
            Map(x => x.fOpen).Column("fOpen");
            Map(x => x.fClose).Column("fClose");
            Map(x => x.fNISOpen).Column("fNISOpen");
            Map(x => x.fNISClose).Column("fNISClose");
           
            //Map(x => x.FAC).Column("FAC");
            Map(x => x.isHoliday).Column("isHoliday");
            //Map(x => x.dAdjPrice).Column("dAdjPrice");

        }
    }
}
