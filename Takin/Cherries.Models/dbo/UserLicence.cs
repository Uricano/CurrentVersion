using Cherries.Models.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.dbo
{
    public class UserLicence
    {
        public int LicenseID { get; set; }
        public string LicenseCode { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime ActivationDate { get; set; }
        public DateTime PurchaseDate { get; set; }
        public bool isTrial { get; set; }
        public int Licensetype { get; set; }
        public LicenceService Service { get; set; }
        public Transactions Transaction { get; set; }
        public IList<StockMarket> Stocks { get; set; }
        public UserLicence()
        {
            Service = new LicenceService();
            Stocks = new List<StockMarket>();
            Transaction = new Transactions();
        }

    }
}
