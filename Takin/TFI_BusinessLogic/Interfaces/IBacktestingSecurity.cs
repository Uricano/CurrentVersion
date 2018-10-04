using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.BusinessLogic.Enums;
using TFI.BusinessLogic.Classes.Optimization.Backtesting;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IBacktestingSecurity : IBaseBL
    {

        // Methods
        void disableCurrentSecurity(cBacktestingSecurities collection); // Makes the current security non-active
        void Init(IPortfolioBL cPort, ISecurity cSec, cDateRange Dates); // Initializes variable

        // Main Properties
        IPortfolioBL Portfolio { get; set; } // Current Portfolio class
        ISecurity Security { get; set; } // Current cSecurity pointer
        ICollectionsHandler ColHandler { get; set; } // Collection handler
        IOptimizationResults EfHandler { get; set; } // Markowitz calculation handler
        IErrorHandler ErrorHandler { get; set; } // Error handler

        // General Properties
        double Weight { get; set; } // Security's weight
        cDateRange DateRange { get; set; } // Date range for backtesting
        enumBacktestingSecTypes SecType { get; set; } // Security Type
        DataTable PriceReturns { get; set; } // Datatable containing all price returns for all securities
        double CurrValue { get; set; } // Current monetary value of security
        double OrigValue { get; set; } // Original monetary value of security

        Boolean isDisabled { get; set; } // Whether the system has disabled the current security

        
    }//Interface
}
