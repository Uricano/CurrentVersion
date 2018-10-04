using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TFI.BusinessLogic.Enums;
using static Cherries.Models.App.PortRiskItem;

namespace Cherries.Models.App
{
    public class PortfolioDetails
    {
        #region Data Members

        // Data variables
        private int m_iPortID = 0;              // Portfolio ID (in DB)
        private String m_strPortCode = "";      // Portfolio code (for investment manager)
        private String m_strPortName = "";      // Portfolio name
        private Boolean m_isManual = false;     // Whether the portfolio was created manually or automatically
        private double m_dEquity = 0D;          // Starting Equity value
        private double m_dCurrEquity = 0D;      // Current Equity value
        private double m_dProfit = 0D;          // Current Profit value
        private cPortRiskItem m_cPreferedRisk;  // Prefered risk level for portfolio (Risk level class)
        private double m_dCurrRiskVal = -1D;     // Selected risk level (in which the optimal portfolio stands)
        private int m_iMaxSecs = 0;             // Maximum number of securities in Optimized Virtual Portfolio
        private int m_iSecsNum = 0;             // Number of securities in optimal portfolio
        private String m_CalcCurrency = "";     // Portfolio currency name
        private enumEfCalculationType m_enumCalcType = enumEfCalculationType.BestTP; // The selected calculation type

        // Date variables
        private DateTime m_dtDateCreated = DateTime.MinValue; // Date portfolio was created
        private DateTime m_dtDateEdited = DateTime.MinValue;  // Date the portfolio was last edited
        
        #endregion Data Members

        #region Properties

        public int ID
        {
            get { return m_iPortID; }
            set { m_iPortID = value; }
        }//ID

        public String Code
        {
            get { return m_strPortCode; }
            set { m_strPortCode = value; }
        }//Code

        public String Name
        {
            get { return m_strPortName; }
            set { m_strPortName = value; }
        }//Name
        public double Profit
        {
            get { return m_dProfit; }
            set { m_dProfit = value; }
        }//Profit

        public double LastProfit { get; set; }
        
        public Boolean isManual
        {
            get { return m_isManual; }
            set { m_isManual = value; }
        }//isLong

        public double Equity
        {
            get { return m_dEquity; }
            set { m_dEquity = value; }
        }//Equity

        public double CurrEquity
        {
            get { return m_dCurrEquity; }
            set { m_dCurrEquity = value; }
        }//CurrEquity

        public DateTime DateCreated
        {
            get { return m_dtDateCreated; }
            set { m_dtDateCreated = value; }
        }//DateCreated

        public DateTime DateEdited
        {
            get { return m_dtDateEdited; }
            set { m_dtDateEdited = value; }
        }//DateEdited
        public cPortRiskItem PreferedRisk
        {
            get { return m_cPreferedRisk; }
            set { m_cPreferedRisk = value; }
        }//PreferedRisk

        public double InitRisk { get; set; }
        public double CurrentStDev
        {
            get { return m_dCurrRiskVal; }
            set { m_dCurrRiskVal = value; }
        }//CurrentStDev

        public int MaxSecs
        {
            get { return m_iMaxSecs; }
            set { m_iMaxSecs = value; }
        }//MaxSecs

        public int SecsNum
        {
            get { return m_iSecsNum; }
            set { m_iSecsNum = value; }
        }//SecsNum

        public string CalcCurrency
        {
            get { return m_CalcCurrency; }
            set { m_CalcCurrency = value; }
        }//CalcCurrency

        public enumEfCalculationType CalcType
        {
            get { return m_enumCalcType; }
            set { m_enumCalcType = value; }
        }//CalcType

        public List<Cherries.Models.Lookup.Sector> PortfolioSectors { get; set; }

        public List<Cherries.Models.App.SecurityData> SecurityData { get; set; }
        public long UserID { get; set; }
        public DateTime LastOptimization { get; set; }
        public double Cash { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        #endregion Properties
    }
}
