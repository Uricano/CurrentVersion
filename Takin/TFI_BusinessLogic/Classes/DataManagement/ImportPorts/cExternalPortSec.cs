using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Used namespaces
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.General;

namespace Cherries.TFI.BusinessLogic.DataManagement.ImportPorts
{
    public class cExternalPortSec
    {

        #region Data members

        // Data variables
        private String m_idSecurity = ""; // Security id, user could enter for Tel Aviv
        private String m_strSymbol = ""; // Security Symbol
        private double m_dQuantity = 0D; // Security quantity (in units)
        private int m_iExchange = 0; // Exchange code for the current security
        private double m_dValue = 0D; // Security's value
        private double m_dWeight = 0D; // Weight of security
        private cSecurity m_objCurrSec; // Security pointer (for optimization calculation)
        private double m_dStartPrice = 0D; // Starting price (from portfolio date range)
        private double m_dEndDate = 0D; // Ending price (from portfolio date range)

        // General variables
        private cImportedPort m_objExtPort; // External portfolio (imported)
        private cErrorHandler m_objErrorHandler; // Error handler class
        private Boolean m_isFound = false; // Whether security was found in server
        //private Boolean m_isModified = false; // Whether the security has been modified (in the edit feature)
        private int m_iModifiedPos = 0; // Modified position of the security in secs for import collection

        #endregion Data members

        #region Constructors, Initialization & Destructor

        public cExternalPortSec(String idSecurity, String strSymbol, double dQuantity, int iExchange, cErrorHandler cErrors, cImportedPort cExtPort)
        {
            m_objExtPort = cExtPort;
            m_objErrorHandler = cErrors;
            m_idSecurity = idSecurity;
            m_strSymbol = strSymbol;
            m_dQuantity = dQuantity;
            m_iExchange = iExchange;
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Methods

        public void setSecurityPrices(cDateRange cDrPort)
        { // Sets the prices of the securities relevant for the portfolio date range
            if (m_objCurrSec == null) return;
            try
            {
                m_dStartPrice = getSecurityPrice(cDrPort.StartDate);
                m_dEndDate = getSecurityPrice(cDrPort.EndDate);
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setSecurityPrices

        private double getSecurityPrice(DateTime dtCurr)
        { // Retrieves the price of the security for the specified date
            Boolean isFound = false;
            double dPriceVal = 0D;
            if (m_objCurrSec.PriceTable.Count > 0)
            { // Only if data exists
                for (int iRows = 0; iRows < m_objCurrSec.PriceTable.Count; iRows++)
                    if (m_objCurrSec.PriceTable[iRows].dDate <= dtCurr)
                    { // Adds found price to collection
                        isFound = true;
                        //dPriceVal = Convert.ToDouble(m_objCurrSec.PriceTable.Rows[iRows][cProperties.PricesFld]);
                        dPriceVal = Convert.ToDouble(m_objCurrSec.PriceTable[iRows].fClose);
                        return dPriceVal;
                    }
                //if (!isFound) return Convert.ToDouble(m_objCurrSec.PriceTable.Rows[m_objCurrSec.PriceTable.Rows.Count - 1][cProperties.PricesFld]);
                if (!isFound) return Convert.ToDouble(m_objCurrSec.PriceTable[m_objCurrSec.PriceTable.Count - 1].fClose);
            }
            return dPriceVal;
        }//getSecurityPrice

        #endregion Methods

        #region Properties

        public Boolean isFound
        { get { return m_isFound; } }//isFound

        public String SecurityId
        {
            get { return m_idSecurity; }
            set { m_idSecurity = value; }
        }//SecurityId

        public String Symbol
        { 
            get { return m_strSymbol; }
            set { m_strSymbol = value; }
        }//Symbol

        public double Quantity
        {
            get { return m_dQuantity; }
            set { m_dQuantity = value; }
        }//Quantity

        public double Weight
        {
            get { return m_dWeight; }
            set { m_dWeight = value; }
        }//Weight

        public double Value
        {
            get { return m_dValue; }
            set { m_dValue = value; }
        }//Value

        public int ExchangeId
        {
            get { return m_iExchange; }
            set { m_iExchange = value; }
        }//ExchangeId

        public cSecurity Security
        { 
            get { return m_objCurrSec; }
            set { m_objCurrSec = value; }
        }//Security

        public double StartPrice
        { get { return m_dStartPrice; } }//StartPrice

        public double EndPrice
        { get { return m_dEndDate; } }//EndPrice

        public cImportedPort ExternalPort
        { get { return m_objExtPort; } }// External portfolio

        #endregion Properties

    }//of class
}
