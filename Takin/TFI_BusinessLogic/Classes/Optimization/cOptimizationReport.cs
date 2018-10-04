using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using Cherries.Models.ViewModel;
using TFI.BusinessLogic.Interfaces;
using Cherries.Models.dbo;
using Cherries.Models.App;
using Cherries.TFI.BusinessLogic.General;

namespace Cherries.TFI.BusinessLogic
{
    public class cOptimizationReport : IOptimizationReport
    {
        private IManagePortfolios managePortfolios;
        private IPriceHandler pricesBL;

        public cOptimizationReport(IManagePortfolios ManagePortfolios, IPriceHandler PricesBL)
        {
            managePortfolios = ManagePortfolios;
            pricesBL = PricesBL;
        }

        public FileDataViewModel Report(int portId, User user, OptimalPortfolio opPort)
        {
            var port = managePortfolios.GetFullPortfolio(portId, user.Currency.CurrencyId, user.Licence.Stocks.Select(x => x.id).ToList());
            StringBuilder csv = new StringBuilder();

            csv.AppendLine("Report optimization:");
            csv.AppendLine("");
            csv.AppendLine("Portfolio name,,Date today");
            csv.AppendLine(port.Details[0].Name + ",," + DateTime.Today.ToString("yyyy/MM/dd"));
            csv.AppendLine("");
            csv.AppendLine("Optimizations results:");
            csv.AppendLine("");

           // cOptimalPort optimal_portf = optimz_res.Portfolios[optimz_res.PortNumA];
            csv.AppendLine("Risk level,Investment,Portfolio value,Profit");
            // TODO: Alex: bug-fix: remove formatting
            //csv.AppendLine(port.Details[0].PreferedRisk.RiskType.ToString() + ",\"" + String.Format("{0:C}", port.Details[0].Equity) + "\",\"" + String.Format("{0:C}", port.Details[0].CurrEquity) + "\",\"" + String.Format("{0:C}", port.Details[0].Profit)+ "\"");
            csv.AppendLine(port.Details[0].PreferedRisk.RiskType.ToString() + ",\"" + port.Details[0].Equity + "\",\"" + port.Details[0].CurrEquity + "\",\"" + port.Details[0].Profit + "\"");
            csv.AppendLine("");
            csv.AppendLine("Risk,Return,Return-to-risk ratio,Diversification");
            csv.AppendLine(cGeneralFunctions.getFormatPercentFromDbl(opPort.Risk ,2) + "," + cGeneralFunctions.getFormatPercentFromDbl(opPort.Return, 3) + "," + cGeneralFunctions.getDoubleFormat(opPort.RateToRisk, 3) + "," +
                cGeneralFunctions.getFormatPercentFromDbl(opPort.Diversification, 1));
            csv.AppendLine(",,Num of secs");
            csv.AppendLine(",," + port.Details[0].SecsNum.ToString());

            csv.AppendLine("");
            csv.AppendLine("Securities results:");
            csv.AppendLine("");
            csv.AppendLine("Name,Symbol,Sector,Quantity,Value,Last price");
            for (int isec = 0; isec < port.Details[0].SecurityData.Count; isec++)
            {
                //DataRow sec = optimz_res.SecuritiesTable.Rows[isec];
                //if (Convert.ToDouble(sec["dWeight"]) != 0.0)
                //{
                
                var sec = port.Details[0].SecurityData[isec];
                var secRate = pricesBL.GetPrices(sec.idSecurity, sec.idCurrency).Where(x => x.Date <= DateTime.Today.AddDays(-1)).FirstOrDefault();
                    csv.AppendLine(sec.strName + "," + sec.strSymbol + "," + sec.sectorName + "," +
                        sec.flQuantity.ToString() + "," + (sec.portSecWeight * port.Details[0].CurrEquity).ToString() + "," + secRate.RateVal.ToString());
               // }
            }

            var vm = new FileDataViewModel();
            vm.Name = "Portfolio_" + port.Details[0].Name + "_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
            vm.Data = Encoding.GetEncoding("Windows-1255").GetBytes(csv.ToString());
            return vm;
        }
    }
}
