using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Used namespaces
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.General;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.TFI.BusinessLogic.Optimization
{
    public class cOptPortSecurity
    {

        #region Data Members

        // Main variables
        private ISecurity m_objSecurity; // Security pointer
        private cOptimalPort m_objOptimalPort; // Optimal portfolio pointer

        // Data variables
        private double m_dWeight = 0D; // Weight of current security in optimal portfolio
        private double m_iQuantity = 0; // Quantity of stocks in current optimal portfolio
        private double m_dValue = 0D; // Monetary value of current security in optimal portfolio
        private double m_dDisplayValue = 0D; // Monetary value of current security in optimal portfolio less CASH
        private double m_dLastPrice = 0D; // Latest price of the security (to calculate its value)

        #endregion Data Members

        #region Constructors, Initialization & Destructor

        public cOptPortSecurity(cOptimalPort cParent, ISecurity cCurrSec, double dWeight, double Equity, double AdjCoeff, double Price)
        {
            m_objOptimalPort = cParent;
            m_objSecurity = cCurrSec;
            m_dWeight = dWeight;
            m_dValue = dWeight * Equity;
            m_iQuantity = m_dValue / Price * AdjCoeff;
            m_dDisplayValue = Math.Floor(m_iQuantity) * Price / AdjCoeff; // whole qty
            m_dDisplayValue = Math.Floor(m_dDisplayValue);                // whole value - no cents/agorot
            m_dLastPrice = Price;
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Properties

        public ISecurity Security
        { get { return m_objSecurity; } }//Security

        public cOptimalPort OptimalPortfolio
        { get { return m_objOptimalPort; } }//OptimalPortfolio

        public double Weight
        { get { return m_dWeight; } }//Weight

        public double Quantity
        { get { return m_iQuantity; } }//Quantity

        public double Value
        { get { return m_dValue; } }//Value

        public double DisplayValue
        { get { return m_dDisplayValue; } }//DisplayValue

        public double Price { get { return m_dLastPrice; } }
        #endregion Properties






    }//of class
}
