using Cherries.Models.App;
using Cherries.Models.dbo;
using Cherries.TFI.BusinessLogic.Securities;
using Ness.DataAccess.Repository;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.BusinessLogic.Interfaces;

namespace TFI.BusinessLogic.Classes.DataManagement.Prices
{
    public class cPriceReturnsHandler
    {

        #region Data Members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Portfolio class pointer
        private ICollectionsHandler m_objColHandler; // Collections handler
        private IErrorHandler m_objErrorHandler; // Error handler class
        private IRepository _repository;

        // Data variables
        private DataTable m_dtAllSecsPrices = null; // Securities price values
        private bool isDisposed = false; // Dispose indicator

        #endregion Data Members

        #region Consturctors, Initialization & Destructor

        public cPriceReturnsHandler(IPortfolioBL cPort, IRepository repository)
        {
            m_objPortfolio = cPort;
            m_objColHandler = m_objPortfolio.ColHandler;
            m_objErrorHandler = m_objPortfolio.cErrorLog;
            _repository = repository;
        }//constructor

        ~cPriceReturnsHandler()
        { Dispose(false); }//destructor

        public void Dispose(bool disposing)
        { // Disposing class variables
            if (disposing)
            { // Managed code
                //m_objOleDBConn = null;
                m_objErrorHandler = null;
                m_objColHandler = null;
                if (m_dtAllSecsPrices != null) m_dtAllSecsPrices.Dispose();
            }
            isDisposed = true;
        }//Dispose

        public void Dispose()
        { // Clear from memory
            Dispose(true);
            GC.SuppressFinalize(this);
        }//Dispose

        public void clearCalcData()
        { // Clears relevant calculation data
            if (m_dtAllSecsPrices != null) m_dtAllSecsPrices.Clear();
        }//clearCalcData

        #endregion Consturctors, Initialization & Destructor

        #region Methods

        #region Global datatable

        public Price getPriceReturn(string secId, string currency, DateTime date)
        { // Retrieves a specific Price return data segment
            // INIT VARIABLES
            List<cSecurity> lstSecurities; // List of securities
            Price pReturn = new Price();
            lstSecurities = (StaticData<cSecurity, ISecurities>.lst == null)? new List<cSecurity>(): StaticData<cSecurity, ISecurities>.lst; // New / Existing list of securities

            cSecurity currSec = lstSecurities.FirstOrDefault(x => x.Properties.PortSecurityId == secId);
            TFI.Entities.dbo.Price price = null;

            // GET SPECIFIC PRICE
            if (currSec == null)
            { // If Security isn't found in StaticData -> take price from repository
                _repository.Execute(session =>
                { price = session.Query<TFI.Entities.dbo.Price>().Where(x => x.idSecurity == secId && x.dDate <= date).FirstOrDefault(); }); // Get price from repository
            } else { // Get price from StaticData
                price = currSec.PriceTable.Where(x => x.dDate <= date).FirstOrDefault();
            }

            if (price != null) pReturn = new Price { dDate = price.dDate, dAdjRtn = (currency == "9001" ? price.fClose : price.fNISClose) };
            return pReturn;
        }//getPriceReturn



        #endregion Global datatable

        #endregion Methods

    }//of class
}
