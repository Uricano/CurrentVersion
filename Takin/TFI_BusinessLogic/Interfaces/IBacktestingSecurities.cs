using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Ness.DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IBacktestingSecurities : IBaseBL
    {

        // Methods


        // Main Properties
        IPortfolioBL Portfolio { get; set; } // Portfolio pointer class
        List<IBacktestingSecurity> BacktestingSecurities { get; set; } // Collection of backtesting securities
        ISecurities Securities { get; set; } // Collection of securities

        // Data Properties
        IBacktestingSecurity TempSec { get; set; } // Temporary security instance (for binary search)
        IRepository repository { get; set; }

        void setBacktestingSecuritiesCollection(ISecurities secsCol, cDateRange cDrRange);
        int GetSecurityInMemoryCount();


    }//of interface
}
