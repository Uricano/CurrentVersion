using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

// Used namespaces
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.GMath;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.TFI.BusinessLogic.DataManagement.ImportPorts
{
    public class cImportedPort
    {

        #region Data members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Current portfolio
        private IErrorHandler m_objErrorHandler; // Error handler class

        // Data variables
        private int m_iPortId = 0; // Portfolio's ID
        private String m_strPortCode = ""; // Portfolio Code
        private String m_strPortName = ""; // Portfolio name
        private double m_dEquity = 0D; // Portfolio equity
        private double m_dRisk = 0D; // Portfolio risk
        private double m_dEr = 0D; // Expected return of portfolio
        private double[] m_arrWeights; // Weights of securities (vector)
        private cDateRange m_drDateRange; // Date range for backtesting
        private List<cExternalPortSec> m_colExtSecurities = new List<cExternalPortSec>(); // Collection of securities in portfolio (External)
        private ISecurities m_colSecurities; // Collection of securities (Full)
        //private List<double> m_colLastPrice = new List<double>(); // Collection of last prices (for our securities)
        private Boolean m_isBacktestingPort = true; // Whether we import a portfolio for backtesting or not
        private String m_strCalcCurrency = "ILS"; // Calculated currency value
        private String m_strPriceFldName = "fClose"; // Name of used prices column

        #endregion Data members

        #region Constructors, Initialization & Destructor

        public cImportedPort(IPortfolioBL cPort)
        {
            m_objPortfolio = cPort;
            m_objErrorHandler = m_objPortfolio.cErrorLog;
            //m_colSecurities = new cSecurities(m_objPortfolio);
            //m_dtDate = DateTime.Now;
            m_drDateRange = new cDateRange(DateTime.Now.AddYears(-2), DateTime.Now);
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region General methods

        private double[] getArrOfWeights()
        { // Retrieves array of weights for securities
            int iStartPos = 0;
            m_arrWeights = new double[m_colSecurities.Count];
            for (int iSecs = 0; iSecs < m_colExtSecurities.Count; iSecs++)
                if ((m_colExtSecurities[iSecs].Security != null) && m_colExtSecurities[iSecs].Security.isActive) 
                //if ((m_colExtSecurities[iSecs].Security != null) && (m_colExtSecurities[iSecs].Weight > 0D)) 
                { m_arrWeights[iStartPos] = m_colExtSecurities[iSecs].Weight; iStartPos++; }

            return m_arrWeights;
        }//getArrOfWeights

        public int getSecurityPos(cSecurity cCurrSec)
        { // Retrieves the position of the security by its symbol

            //TODO: Don't we have to check also by MARKET ID????
            for (int iSecs = 0; iSecs < m_colExtSecurities.Count; iSecs++)
            {
                //if (m_colExtSecurities[iSecs].Symbol == cCurrSec.Properties.SecuritySymbol)
                if (m_colExtSecurities[iSecs].Symbol == cCurrSec.Properties.SecuritySymbol && m_colExtSecurities[iSecs].ExchangeId == cCurrSec.Properties.Market.ID)
                    return iSecs;
            }
            return -1;
        }//getSecurityPosBySymbol

        #endregion General methods

        #region Calculation methods

        public void calcExtSecsWeights()
        { // Fills the external securities with the proper data
            try
            {
                double l_AdjCoeff = 1;
                double dCurrVal = 0D; m_dEquity = 0D;
                for (int iSecs = 0; iSecs < m_colExtSecurities.Count; iSecs++)
                { // Goes through securities
                    m_colExtSecurities[iSecs].setSecurityPrices(m_drDateRange);

                    // If Price is in agorot, divide by 100
                    l_AdjCoeff = (m_colExtSecurities[iSecs].ExternalPort.CalcCurrency == "9999") ? 100.0 : 1;

                    dCurrVal = m_colExtSecurities[iSecs].Quantity * m_colExtSecurities[iSecs].EndPrice / l_AdjCoeff;
                    m_colExtSecurities[iSecs].Value = dCurrVal;
                    m_dEquity += dCurrVal;
                }

                for (int iSecs = 0; iSecs < m_colExtSecurities.Count; iSecs++)
                    m_colExtSecurities[iSecs].Weight = m_colExtSecurities[iSecs].Value / (double)m_dEquity;
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//calcExtSecsWeights

        public PointF calcExtPortRiskReturn(double[] arrRates, double[,] arrCovar)
        { // Calculates the risk and return for the client's external portfolio
            double[] arrWeights = getArrOfWeights();
            double[,] finalRisks = cMath.getMultipliedMatrix(cMath.getMultipliedMatrix(cMath.getTransposedVector(arrWeights), arrCovar), cMath.getMatrixVersionOfArr(arrWeights));
            double[,] finalRates = cMath.getMultipliedMatrix(cMath.getTransposedVector(arrWeights), cMath.getMatrixVersionOfArr(arrRates));

            m_dRisk = System.Math.Sqrt(finalRisks[0, 0]) * Math.Sqrt(cProperties.getCurrentScaling());
            m_dEr = finalRates[0, 0] * cProperties.getCurrentScaling();
            return new PointF((float)m_dRisk, (float)m_dEr);
        }//calcExtPortStatistics

        public double getPortfolioProfit()
        { // Calculates profit for all securities participating
            double dFinalVal = 0D;
            try
            {
                double l_AdjCoeff = 1;
                for (int iSecs = 0; iSecs < m_colExtSecurities.Count; iSecs++)
                {
                    // If Price is in agorot, divide by 100
                    l_AdjCoeff = (m_colExtSecurities[iSecs].ExternalPort.CalcCurrency == "9999") ? 100.0 : 1;

                    dFinalVal += (m_colExtSecurities[iSecs].Quantity * m_colExtSecurities[iSecs].EndPrice / l_AdjCoeff) - (m_colExtSecurities[iSecs].Quantity * m_colExtSecurities[iSecs].StartPrice / l_AdjCoeff);
                }
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
            return dFinalVal;
        }//getPortfolioProfit

        #endregion Calculation methods

        #endregion Methods

        #region Properties

        public int ID
        {
            get { return m_iPortId; }
            set { m_iPortId = value; }
        }//ID

        public String Code
        {
            get { return m_strPortCode; }
            set { m_strPortCode = value; }
        }//Code

        public String PortName
        {
            get { return m_strPortName; }
            set { m_strPortName = value; }
        }//PortName

        public double Equity
        {
            get { return m_dEquity; }
            set { m_dEquity = value; }
        }//Equity

        public double Risk
        { get { return m_dRisk; } }//Risk

        public cDateRange DateRange
        {
            get { return m_drDateRange; }
            set { m_drDateRange = value; }
        }//Date

        public ISecurities Securities
        {
            get { return m_colSecurities; }
            set { m_colSecurities = value; }
        }//Securities

        public List<cExternalPortSec> ExternalSecurities
        {
            get { return m_colExtSecurities; }
            set { m_colExtSecurities = value; }
        }//ExternalSecurities

        public Boolean isBacktesting
        {
            get { return m_isBacktestingPort; }
            set { m_isBacktestingPort = value; }
        }//isBacktesting

        public String CalcCurrency
        {
            get { return m_strCalcCurrency; }
            set { m_strCalcCurrency = value; }
        }//CalcCurrency

        public String PricesFld
        {
            get { return m_strPriceFldName; }
            set { m_strPriceFldName = value; }
        }//PricesFld

        #endregion Properties

    }//of class
}
