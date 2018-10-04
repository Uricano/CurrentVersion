using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Used namespaces
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.GMath.StaticMethods;
using TFI.BusinessLogic.Interfaces;
using Cherries.Models.App;

namespace Cherries.TFI.BusinessLogic.Optimization
{
    public class cOptimalPort
    {

        #region Data Members

        // Main variables
        private IErrorHandler m_objErrorHandler; // Error handler

        // Data variables
        private List<cOptPortSecurity> m_colSecurities = new List<cOptPortSecurity>(); // Collection of participating securities (in current portfolio)

        // Portfolio variables
        private int m_iPortPos = 0; // Position of portfolio in collection
        private double m_dPortRisk = 0D; // Portfolio calculated risk
        private double m_dPortReturn = 0D; // Portfolio calculated return
        private double m_dRateToRisk = 0D; // Rate to Risk Ratio
        private double m_dDiversification = 0D; // Diversification value
        private double m_dSharpe = 0D; // Sharpe value
        private double m_cash = 0D;

        #endregion Data Members

        #region Constructors, Initialization & Destructor

        public cOptimalPort(double[,] dWeights, int iPos, double dRisk, double dReturn, ISecurities cSecsCol, IErrorHandler cErrors, double Equity, double AdjCoeff, string CalcCurrency)
        {
            m_objErrorHandler = cErrors;
            m_iPortPos = iPos;
            m_dPortRisk = dRisk;
            m_dPortReturn = dReturn;

            try
            {
                cOptimalStaticCalcs op = new cOptimalStaticCalcs();
                setSecuritiesCollection(cSecsCol, dWeights, Equity, AdjCoeff, CalcCurrency);
               
                m_dRateToRisk = m_dPortReturn / m_dPortRisk;
                m_dDiversification = op.getDiversificationValue(dWeights, cSecsCol, iPos, m_dPortRisk);

                double dSecValue;
                double dCurrCash;
                m_cash = 0;
                for (var i = 0; i < m_colSecurities.Count; i++)
                {
                    dSecValue = Math.Floor(m_colSecurities[i].Quantity) * m_colSecurities[i].Price / AdjCoeff;
                    dCurrCash = (m_colSecurities[i].Quantity - Math.Floor(m_colSecurities[i].Quantity)) * m_colSecurities[i].Price / AdjCoeff;
                    // cash : sum - after making qty whole + cents/agorot of security value 
                    m_cash += dCurrCash + dSecValue - Math.Floor(dSecValue); 
                }
            }
            catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Methods

        private void setSecuritiesCollection(ISecurities cSecsCol, double[,] dWeights, double Equity, double AdjCoeff, string CalcCurrency)
        { // Creates collection of securities for current portfolio
            try
            {
                m_colSecurities.Clear();
                DateTime CurrDate = DateTime.Today.AddDays(-1);
                for (int iSecs = 0; iSecs < dWeights.GetLength(1); iSecs++)
                    if (dWeights[m_iPortPos, iSecs] > 0D )
                    {
                        var Price = getSecurityPrice(cSecsCol[iSecs], CurrDate, CalcCurrency);
                        var sec = getPortfolioSecurity(cSecsCol[iSecs], dWeights[m_iPortPos, iSecs], Equity, AdjCoeff, Price);
                        //if (sec.Quantity >= 1)        // TODO: WHAT IS THAT?????? !!!!!!!!!!!!
                        m_colSecurities.Add(sec);    

                    }
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setOptimalPortfoliosCollection

        private cOptPortSecurity getPortfolioSecurity(ISecurity cCurrSec, double dWeight, double Equity, double AdjCoeff, double Price)
        { // Retrieves portfolio security instance
          // TODO: Check if used for display Values in the sec list. They calculated without CASH consideration.
          // But if used both for dispplay and update database, then have to calculate CASH for display only. 
            cOptPortSecurity cSec = new cOptPortSecurity(this, cCurrSec, dWeight, Equity, AdjCoeff, Price);
            

            return cSec;
        }//getPortfolioSecurity

        private double getSecurityPrice(ISecurity CurrSec, DateTime dtCurr, string CalcCurrency)
        { // Retrieves the price of the security for the specified date
            Boolean isFound = false;
            double dPriceVal = 0D;
            if (CurrSec.PriceTable.Count > 0)
            { // Only if data exists
                for (int iRows = 0; iRows < CurrSec.PriceTable.Count; iRows++)
                    if (Convert.ToDateTime(CurrSec.PriceTable[iRows].dDate) <= dtCurr)
                    { // Adds found price to collection
                        isFound = true;
                        dPriceVal = (CalcCurrency == "9999") ? Convert.ToDouble(CurrSec.PriceTable[iRows].fNISClose) : Convert.ToDouble(CurrSec.PriceTable[iRows].fClose); //Convert.ToDouble(CurrSec.PriceTable[iRows].dAdjPrice);
                        return dPriceVal;
                    }
                if (!isFound) return (CalcCurrency == "9999") ? Convert.ToDouble(CurrSec.PriceTable[CurrSec.PriceTable.Count - 1].fNISClose) : Convert.ToDouble(CurrSec.PriceTable[CurrSec.PriceTable.Count - 1].fClose);
            }
            return dPriceVal;
        }//getSecurityPrice
        #endregion Methods

        #region Properties

        public List<cOptPortSecurity> Securities
        { get { return m_colSecurities; } }//Securities

        public double Risk
        { get { return m_dPortRisk; } }//Risk

        public double Return
        { 
            get { return m_dPortReturn; }
            set { m_dPortReturn = value; }
        }//Return

        public double RateToRisk
        { get { return m_dRateToRisk; } }//RateToRisk

        public double Diversification
        { get { return m_dDiversification; } }//Diversification

        public double Sharpe
        { get { return m_dSharpe; } }//Sharpe

        public double Cash { get { return m_cash; } }

        public static explicit operator cOptimalPort(OptimalPortfolio v)
        {
            throw new NotImplementedException();
        }
        #endregion Properties

    }//of class
}
