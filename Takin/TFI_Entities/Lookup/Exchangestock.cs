using System;
using System.Text;
using System.Collections.Generic;


namespace TFI.Entities.Lookup {
    
    public class Exchangestock
    {
        public virtual int idExchangePackage { get; set; }
        public virtual int idStockMarket { get; set; }
        public virtual Selexchanges tbl_SelExchanges { get; set; }
        public virtual SelStockExchange tblSel_StockMarkets { get; set; }
        #region NHibernate Composite Key Requirements
        public override bool Equals(object obj) {
			if (obj == null) return false;
			var t = obj as Exchangestock;
			if (t == null) return false;
			if (idExchangePackage == t.idExchangePackage
			 && idStockMarket == t.idStockMarket)
				return true;

			return false;
        }
        public override int GetHashCode() {
			int hash = GetType().GetHashCode();
			hash = (hash * 397) ^ idExchangePackage.GetHashCode();
			hash = (hash * 397) ^ idStockMarket.GetHashCode();

			return hash;
        }
        #endregion
    }
}
