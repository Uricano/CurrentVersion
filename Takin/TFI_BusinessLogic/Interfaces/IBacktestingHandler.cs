using Cherries.Models.App;
using Cherries.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IBacktestingHandler : IBaseBL
    {
        void setBacktestingPortfolio(IPortfolioBL cPort, DateTime dtStartDate, DateTime dtEndDate, double dEquity);
        //void fillPortAndBMarksReturnsTable();

        //List<PriceReturn> GetPriceReturnsInDateRangeInSecCol(IBacktestingSecurities cSecsCol, DateTime fromDT, DateTime toDT, string calcCurrency);

        //void calculateSecuritiesDatatable();
        //void calculateSecuritiesDatatable(DateTime dtStart, DateTime dtEnd);
        //void calculateBenchmarkRiskAndRefreshTables();
        //void fillPortAndBMarksReturnsTable(DateTime dtStart, DateTime dtEnd);

        //BackTestingViewModel calculateBacktesting(IPortfolioBL cPort, DateTime dtStartDate, DateTime dtEndDate, double dEquity, List<string> becnhMarkIDs);
        BackTestingViewModel calculateNewBacktesting(IPortfolioBL cPort, DateTime dtStartDate, DateTime dtEndDate, double dEquity, List<string> becnhMarkIDs);
        //BackTestingViewModel getBacktestingPortfolio(IPortfolioBL cPort, DateTime dtStartDate, DateTime dtEndDate, double dEquity);
        BackTestingViewModel getNewBacktestingPortfolio(IPortfolioBL cPort, DateTime dtStartDate, DateTime dtEndDate, double dEquity);

        ISecurities Benchmarks { get; }

    }
}
