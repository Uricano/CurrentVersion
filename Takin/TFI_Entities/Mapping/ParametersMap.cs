using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.Lookup;

namespace TFI.Entities.Mapping
{
    public class ParametersMap : ClassMap<Parameters>
    {
        public ParametersMap()
        {
            Table("tbl_Parameters");
            Id(x => x.Name).Column("Name");
            Map(x => x.Type).Column("Type");
            Map(x => x.Value).Column("Value");
        }
    }
}
