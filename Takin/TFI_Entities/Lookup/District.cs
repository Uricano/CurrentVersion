using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.Entities.Lookup
{
    public class District
    {
        public virtual int idDistrict { get; set; }
        public virtual Country Country { get; set; }
        public virtual string strDistrictName { get; set; }
    }
}
