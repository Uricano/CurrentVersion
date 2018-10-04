using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TFI.BusinessLogic.Enums;

namespace Cherries.Models.dbo
{
    public class BacktestingSecurity
    {

        Security Security { get; set; } // Current cSecurity pointer

        double Weight { get; set; } // Security's weight
        public DateTime? dtStartDate { get; set; } // Date range for backtesting
        public DateTime? dtEndDate { get; set; } // Date range for backtesting
        enumBacktestingSecTypes SecType { get; set; } // Security Type
        DataTable PriceReturns { get; set; } // Datatable containing all price returns for all securities
        double CurrValue { get; set; } // Current monetary value of security
        double OrigValue { get; set; } // Original monetary value of security

    }//Of Class
}
