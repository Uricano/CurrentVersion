using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cherries.TFI.BusinessLogic.Securities;
using TFI.BusinessLogic.Interfaces;
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.General;
using TFI.BusinessLogic.Bootstraper;
using Ness.DataAccess.Repository;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.Models.dbo;
using TFI.BusinessLogic.Enums;
using NHibernate.Linq;
using System.Linq;
//using Ness.DataAccess.DocumentDB;
using System.Threading.Tasks;
using Ness.Utils;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        
        [TestMethod]
        public void TestMethod1()
        {
            //IErrorHandler err = new cErrorHandler();
            //IPortfolioBL port = new cPortfolio(err);
            //cSecurities secs = new cSecurities(port);
            //DocumentDBHandler.Initialize();
            //var sList = secs.GetTopSecurities(500, new System.Collections.Generic.List<int> { 1, 3, 4, 5 });
            //foreach (var s in sList)
            //{
            //    var CurrentSec = getCurrSecurity(s, port);
            //    Task.Factory.StartNew(() => DocumentDBHandler.CreateItemAsync<cSecurity>(CurrentSec)); 
            //}
            
        }

        private cSecurity getCurrSecurity(Security security, IPortfolioBL cCurrPort)
        { // Retrieves an instance of a security based on its datarow info
            ICollectionsHandler cColHandler = cCurrPort.ColHandler;

            cSecurity cCurrSec = new cSecurity(cCurrPort, security.strName, security.strSymbol);//111An
            cCurrSec.Properties.PortSecurityId = security.idSecurity;

            //cCurrSec.Properties.HebName = security.HebName;

            //cCurrSec.FAC = Convert.ToDouble(security.FAC);

            if (cCurrSec.FAC <= 0D) cCurrSec.FAC = 1D;

            cCurrSec.AvgYield = security.AvgYield;
            cCurrSec.StdYield = security.StdYield;

            cCurrSec.AvgYieldNIS = security.AvgYieldNIS;
            cCurrSec.StdYieldNIS = security.StdYieldNIS;

            cCurrSec.ValueUSA = security.dValueUSA;
            cCurrSec.ValueNIS = security.dValueNIS;

            cCurrSec.WeightUSA = security.WeightUSA;
            cCurrSec.WeightNIS = security.WeightNIS;

            //cCurrSec.Properties.ISIN = security.strISIN;

            cCurrSec.DateRange = new cDateRange(security.dtPriceStart, security.dtPriceEnd);
            cCurrSec.setSecurityActivity(true);

            //lock (lockObject)
            //{
            cCurrSec.Properties.Sector = cColHandler.getCatItemByID(enumCatType.Sector, security.idSector,
                        cColHandler.Sectors);

                cCurrSec.Properties.Market = cColHandler.getCatItemByID(enumCatType.StockMarket, security.idMarket, cColHandler.Markets);
                cCurrSec.Properties.MarketName = getSecMarketName(security.idMarket);

                cCurrSec.Properties.SecurityType = cColHandler.getCatItemByID(enumCatType.SecurityType,
                       security.idSecurityType, cColHandler.SecTypes);
            //}
            var priceRepository = Resolver.Resolve<IRepository>();
            try
            {
                priceRepository.Execute(session =>
                {
                    //cCurrSec.PriceTable = session.Query<TFI.Entities.dbo.Price>().Where(x => x.Securities.idSecurity == security.idSecurity).OrderByDescending(x => x.dDate).ToList();
                });
            }
            catch (Exception ex)
            {
                //m_objErrorHandler.LogInfo(ex);
            }
            Resolver.Release(priceRepository);
            return cCurrSec;
        }//getCurrSecurity

        private String getSecMarketName(int idMarket)
        { // Gets the market name for display (for the current security)
            if (idMarket == 1) return "TASE";
            return "USA";
        }//getSecMarketName

    }
}
