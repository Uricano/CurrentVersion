using System; 
using System.Collections.Generic; 
using System.Text; 
using FluentNHibernate.Mapping;
using TFI.Entities.Lookup; 

namespace TFI.Entities.Mapping {
    
    
    public class ExchangestockMap : ClassMap<Exchangestock> {
        
        public ExchangestockMap() {
			Table("tbl_ExchangeStock");
			LazyLoad();
			CompositeId().KeyProperty(x => x.idExchangePackage, "idExchangePackage")
			             .KeyProperty(x => x.idStockMarket, "idStockMarket");
			References(x => x.tbl_SelExchanges).Column("idExchangePackage");
			References(x => x.tblSel_StockMarkets).Column("idStockMarket");
        }
    }
}
