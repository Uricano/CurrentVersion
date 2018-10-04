using Cherries.Models.dbo;
using Cherries.Models.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.Command
{
    public class UpdateLicenseCommand
    {
        public Transactions Transaction { get; set; }
        public IList<StockMarket> Stocks { get; set; }
        public int LicenseID { get; set; }
        public int Idlicservice { get; set; }
        public Int64 UserID { get; set; }
        public double SumInServer { get; set; }
    }
}
