using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using Cherries.Services.Interfaces;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.GMath.EF;
using Cherries.TFI.BusinessLogic.Portfolio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TFI.BusinessLogic.Enums;
using TFI.BusinessLogic.Interfaces;
using Cherries.Models.App;
using Cherries.TFI.BusinessLogic.Securities;
//using Ness.DataAccess.Repository;
//using TFI.Entities.dbo;
//using TFI.BusinessLogic.Bootstraper;
//using NHibernate.Linq;

namespace Cherries.Services
{
    public class OptimizationService : IOptimizationService
    {
        private IPortfolioBL cPort;
        private ISecurities SecuritiesBL;
        private IOptimizationReport OptimizationReport;
        private static object lockObject = new object();

        public OptimizationService(IPortfolioBL portBL, ISecurities securitiesBL, IOptimizationReport optimizationReport)
        {
            cPort = portBL;
            SecuritiesBL = securitiesBL;
            OptimizationReport = optimizationReport;
        }


        public static void StartProcess(string path)
        {
            cQLDService.StartProcess(path);
        }

        public static void EndProcess()
        {
            cQLDService.compiler.Kill();
        }
        public OptimalPortoliosViewModel GetPortfolioOptimazation(User user, OptimazationQuery query)
        {
            OptimalPortoliosViewModel vm = new OptimalPortoliosViewModel();
            cPort.openExistingPortfolio(query.PortID, false);
            lock (lockObject)
            {
                var stocks = query.Exchanges != null && query.Exchanges.Count > 0
                    && query.Exchanges.Count <= user.Licence.Stocks.Count ? query.Exchanges : user.Licence.Stocks.Select(t => t.id).ToList();
                //if (!cPort.instantiatePortfolioVariables(true, cPort.Details.CalcCurrency, stocks , query.Securities))
                if (!cPort.instantiateVariablesForPortfolio(true, cPort.Details.CalcCurrency, stocks, query.Securities))
                    vm.Messages.Add(new Models.App.Message { LogLevel = Models.App.LogLevel.Error, Text = "Error Get Portfolio Data. Please Contanct System Administrator" });
                else
                {
                    cPort.ColHandler.setDisabledSecsToActive();

                    if (query.Securities.Count > 0)
                        cPort.ColHandler.ActiveSecs = getFilteredSecByCustomList(query);    //custom portfolio build
                    else  // filter Active securities from Securities by exchangesIds and risk level: sec.stdYield
                        cPort.ColHandler.ActiveSecs = getFilteredSecByExchangesAndRisk(query);


                    cPort.Details.CalcType = query.CalcType;
                   // cPort.Classes.Optimizer.SecuritiesCol = cPort.ColHandler.ActiveSecs;

                    ISecurities cSecsCol = cPort.ColHandler.ActiveSecs;
                    //vm = cPort.Classes.Optimizer.calculateNewEfficientFrontier(ref cSecsCol, cPort.ColHandler.Benchmarks, new cDateRange(DateTime.Today.AddYears(-cProperties.DatesInterval), DateTime.Today), true, false);
                    vm = cPort.Classes.Optimizer.calculateNewEfficientFrontier(ref cSecsCol, cPort.ColHandler.Benchmarks, new cDateRange(DateTime.Today.AddYears(-3).AddDays(-1), DateTime.Today.AddDays(-1)), true, false);
                    cPort.ColHandler.ActiveSecs = cSecsCol;
                }
            }
            return vm;
        }//GetPortfolioOptimazation

        private ISecurities getFilteredSecByCustomList(OptimazationQuery query)
        {
            List<string> missingSecs = new List<string>();

            ISecurities newColl = new cSecurities(cPort);

            for (int i = 0; i < query.Securities.Count; i++)
            {
                var sec = cPort.ColHandler.Securities.getSecurityByIdNormalSearch(query.Securities[i]);
                if (sec != null)
                    newColl.Add(sec);
                else
                    missingSecs.Add(query.Securities[i]);
            }

            if(missingSecs.Count > 0)
            {
                cPort.ColHandler.addMissingSecurities(newColl,  missingSecs);
            }
            //********************************************************
            //IRepository repository;
            //repository = Resolver.Resolve<IRepository>();
            //TopSecuritiesViewModel vm = new TopSecuritiesViewModel();
            //repository.Execute(session =>
            //{
            //    var securities = session.Query<TopSecurities>().Where(x => query.Securities.Contains(x.idSecurity));

            //    //return AutoMapper.Mapper.Map<List<Models.dbo.Security>>(topSecs);

            //    //TODO: have to do mapping probably
            //    newColl = newColl.GetCustomSecurities(securities);
            //});
            //********************************************************
            return newColl;
        }



        private ISecurities getFilteredSecByExchangesAndRisk(OptimazationQuery query)
        {
            ISecurities newColl = new cSecurities(cPort);
            for (int i = 0; i < cPort.ColHandler.Securities.Count ; i++)
            {
                //if (cPort.ColHandler.Securities[i].StdYield * Math.Sqrt(52) <= query.Risk && query.Exchanges.Contains(cPort.ColHandler.Securities[i].Properties.Market.ID))
                    if (query.Exchanges.Contains(cPort.ColHandler.Securities[i].Properties.Market.ID))
                        if (query.Risk > 0.14 || (query.Risk <= 0.14 && cPort.ColHandler.Securities[i].RatesClass.PriceReturns.Count > 105))
                        newColl.Add(cPort.ColHandler.Securities[i]);
            }

            return newColl;
        }

        //private void setSecuritiesCollection()
        //{ // Sets the collection used for the optimization
        //    switch (cPort.Details.CalcType)
        //    { // Sets the proper collection for optimization
        //        case enumEfCalculationType.BestTP: cPort.Classes.Optimizer.SecuritiesCol = cPort.ColHandler.Securities; break;
        //        case enumEfCalculationType.BestRisk: cPort.Classes.Optimizer.SecuritiesCol = cPort.ColHandler.Securities; break;
        //        case enumEfCalculationType.Custom: cPort.Classes.Optimizer.SecuritiesCol = cPort.ColHandler.ActiveSecs; break;
        //        case enumEfCalculationType.SecurityStd: cPort.Classes.Optimizer.SecuritiesCol = cPort.ColHandler.SecuritiesByRisk; break;
        //    }
        //}//setSecuritiesCollection

        public void UpdateSecurities(HttpContext current)
        {
            HttpContext.Current = current;
            cPort.InitCollectionObject("", null);
            cPort.ColHandler.SetSecurites();
        }

        public FileDataViewModel Export(int portId, User user, OptimalPortfolio opPort)
        {
            var vm = OptimizationReport.Report(portId, user, opPort);
            return vm;
        }
    }
}
