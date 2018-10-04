using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.Entities.dbo
{
    public class BMPrice
    {
        public virtual int idPrice { get; set; }
        public virtual string idSecurity { get; set; }
        public virtual DateTime dDate { get; set; }
        //public virtual double? fVolume { get; set; }
        public virtual double? fOpen { get; set; }       //LR:already adjusted from Intrinio
        public virtual double? fClose { get; set; }      //LR:already adjusted from Intrinio
        //public virtual double? fHigh { get; set; }
        //public virtual double? fLow { get; set; }
        //public virtual double? FAC { get; set; }
        public virtual bool? isHoliday { get; set; }
        public virtual double? fNISClose { get; set; }  //LR:already adjusted from Intrinio
        public virtual double? fNISOpen { get; set; }   //LR:already adjusted from Intrinio
        //public virtual double? dAdjPrice { get; set; }
        public virtual double? dAdjRtn { get; set; }  //LR: have to load it from tbl_PriceReturns - calculates at night
        //public virtual double? dFacAcc { get; set; }
        //public virtual double? dFacAccAll { get; set; }

        #region NHibernate Composite Key Requirements

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var t = obj as BMPrice;
            if (t == null) return false;
            if (idSecurity == t.idSecurity
             && dDate == t.dDate)
                return true;

            return false;
        }//Equals

        public override int GetHashCode()
        {
            int hash = GetType().GetHashCode();
            hash = (hash * 397) ^ idSecurity.GetHashCode();
            hash = (hash * 397) ^ dDate.GetHashCode();

            return hash;
        }//GetHashCode

        #endregion NHibernate Composite Key Requirements

    }
}
