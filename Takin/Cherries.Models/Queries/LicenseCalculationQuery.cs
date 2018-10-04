using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Cherries.Models.Queries
{
    public class LicenseCalculationQuery
    {
        public int ServiceID { get; set; }
        public int StockCount { get; set; }
        public string Copon { get; set; }
    }
}
