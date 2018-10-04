using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.Entities.dbo
{
    public class PriceReturn : EntityBase
    {
        public virtual string idSecurity { get; set; }
        public virtual int idCurrency { get; set; }
        public virtual DateTime dtDate { get; set; }
        public virtual double? dReturn { get; set; }
        public virtual double? fAdjClose { get; set; }
        public virtual double? fAdjClosePrevWeek { get; set; }

        #region NHibernate Composite Key Requirements

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var t = obj as PriceReturn;
            if (t == null) return false;
            if (idSecurity == t.idSecurity
             && idCurrency == t.idCurrency  //LR
             && dtDate == t.dtDate)
                return true;

            return false;
        }//Equals

        public override int GetHashCode()
        {
            int hash = GetType().GetHashCode();
            hash = (hash * 397) ^ idSecurity.GetHashCode();
            hash = (hash * 397) ^ idCurrency.GetHashCode();     //LR
            hash = (hash * 397) ^ dtDate.GetHashCode();

            return hash;
        }//GetHashCode

        #endregion NHibernate Composite Key Requirements

    }//of Entity
}
