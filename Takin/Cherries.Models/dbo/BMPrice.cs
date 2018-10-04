using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.dbo
{
    public class BMPrice
    {
        public int idPrice { get; set; }          //LR: why do we need it?
        public string idSecurity { get; set; }
        public DateTime dDate { get; set; }
        //public double? fVolume { get; set; }
        public double? fOpen { get; set; }
        public double? fClose { get; set; }
        //public double? fHigh { get; set; }
        //public double? fLow { get; set; }
        //public double? FAC { get; set; }
        public bool? isHoliday { get; set; }
        public double? fNISClose { get; set; }
        public double? fNISOpen { get; set; }
        //public double? dAdjPrice { get; set; }
        public double? dAdjRtn { get; set; }
        //public double? dFacAcc { get; set; }
        //public double? dFacAccAll { get; set; }
    }
}
