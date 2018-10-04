using System; 
using System.Collections.Generic; 
using System.Text; 
using FluentNHibernate.Mapping;
using TFI.Entities.Lookup; 

namespace TFI.Entities.Mapping {
    
    
    public class SelStockmarketsMap : ClassMap<SelStockExchange> {
        
        public SelStockmarketsMap() {
			Table("v_tbSel_StockMarkets");
			LazyLoad();
			Id(x => x.id).GeneratedBy.Identity().Column("id");
			Map(x => x.strName).Column("strName");
			References(x => x.Currency).Column("idCurrency");
			Map(x => x.strHebName).Column("strNameHeb");
            Map(x => x.LastUpdate).Column("LastUpdate");
            Map(x => x.IsActive).Column("isActive");
        }
    }
}
