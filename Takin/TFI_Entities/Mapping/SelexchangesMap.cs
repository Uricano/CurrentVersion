using System; 
using System.Collections.Generic; 
using System.Text; 
using FluentNHibernate.Mapping;
using TFI.Entities.Lookup; 

namespace TFI.Entities.Mapping {
    
    
    public class SelexchangesMap : ClassMap<Selexchanges> {
        
        public SelexchangesMap() {
			Table("tbl_SelExchanges");
			LazyLoad();
			Id(x => x.idExchangePackage).GeneratedBy.Identity().Column("idExchangePackage");
			Map(x => x.strName).Column("strName");
			Map(x => x.idCodes).Column("idCodes");
            HasManyToMany(x => x.Markets).Table("tbl_ExchangeStock").ParentKeyColumn("idExchangePackage").ChildKeyColumn("idStockMarket").Cascade.All();
        }
    }
}
