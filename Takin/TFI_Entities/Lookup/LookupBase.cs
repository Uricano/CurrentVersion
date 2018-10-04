using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.Entities.Lookup
{
    public class LookupBase
    {
        public virtual int id { get; set; }
        public virtual string strName { get; set; }
        public virtual string strHebName { get; set; }
        public virtual bool IsActive { get; set; }
    }
}
