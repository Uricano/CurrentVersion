using System; 
using System.Collections.Generic; 
using System.Text; 
using FluentNHibernate.Mapping;
using TFI.Entities.Lookup; 

namespace TFI.Entities.Mapping {
    
    
    public class SectorMap : ClassMap<Sector> {
        
        public SectorMap() {
			Table("v_tbSel_Sectors");
			LazyLoad();
			Id(x => x.id).GeneratedBy.Identity().Column("id");
			Map(x => x.strName).Column("strName");
			Map(x => x.strHebName).Column("strNameHeb");
            Map(x => x.IsActive).Column("isActive");
        }
    }
}
