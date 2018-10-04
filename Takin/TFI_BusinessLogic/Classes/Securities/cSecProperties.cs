using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

//Used namespaces
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Collections;
using Cherries.TFI.BusinessLogic.Portfolio;
using TFI.BusinessLogic.Enums;
using Cherries.TFI.BusinessLogic.Categories;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.TFI.BusinessLogic.Securities
{
    public class cSecProperties
    {

        #region Data members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Portfolio pointer
        private IErrorHandler m_objErrorHandler; // Error handler class
        private string m_strSecName; // Security name
        private string m_strHebName; // Hebrew security name
        private string m_strSecSymbol; // Security Symbol
        private string m_strMarket; // Security Market
        private string m_strISIN; // ISIN security code
        private String m_iPortSecId; // portfolio's security id
        private int m_iNumericId; // Security ID (Numeric value)

        // Category variables
        private ICategoryItem m_catSector; // Category info class (Sector)
        private ICategoryItem m_catMarket; // Category info class (Exchange)
        private ICategoryItem m_catSecType; // Category info class (Security Type)
        //private cCategoryItem m_catCurrency;  // Category info class (Currency)
        private Color m_clrSecColor; // Security display color

        #endregion Data members

        #region Constructors, Initialization & Destructor

        public cSecProperties(IErrorHandler cErrors, IPortfolioBL cPort)
        {
            m_objPortfolio = cPort;
            m_objErrorHandler = cErrors;
            m_catSector = new cCategoryItem(enumCatType.Sector, "", -1, m_objErrorHandler, cPort);
            //m_catCurrency = new cCategoryItem(enumCatType.Currency, "", -1, m_objErrorHandler);
            m_catMarket = new cCategoryItem(enumCatType.StockMarket, "", -1, m_objErrorHandler, cPort);
            m_catSecType = new cCategoryItem(enumCatType.SecurityType, "", -1, m_objErrorHandler, cPort);
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region General methods

        public cSecProperties Clone()
        { // Clones properties
            cSecProperties cFinalProps = new cSecProperties(m_objErrorHandler, m_objPortfolio);
            cFinalProps.SecurityName = this.m_strSecName;
            cFinalProps.SecuritySymbol = this.m_strSecSymbol;
            cFinalProps.ISIN = this.m_strISIN;
            cFinalProps.PortSecurityId = this.m_iPortSecId;
            cFinalProps.NumericID = this.m_iNumericId;
            cFinalProps.SecColor = this.m_clrSecColor;
            cFinalProps.Sector = this.m_catSector;
            cFinalProps.Market = this.m_catMarket;
            cFinalProps.SecurityType = this.m_catSecType;
            //cFinalProps.Currency = this.m_catCurrency;

            return cFinalProps;
        }//Clone

        public String getSecName(String oldName)
        {
            if (string.IsNullOrEmpty(oldName))
                return "";
            return oldName.Replace("'", "").Trim();
        }//getSecName

        private String getSecDisplayValue()
        { // Returns the display value (symbol / name) of current security
            switch (cProperties.DisplayOfSecurity)
            {
                case enumSecurityDisplay.Ticker: return this.m_strSecSymbol;
                case enumSecurityDisplay.Name: return ((!cProperties.isIsraelOnly) ? this.m_strSecName : this.m_strHebName);
                default: return this.m_strSecSymbol;
            }
        }//getSecDisplayValue

        //public String getSecurityCurrencyFormat()
        //{ // Retrieves the currency format suitable for current security
        //    String strSymbol = "$";
        //    switch (m_catCurrency.ID)
        //    {
        //        case 1: strSymbol = "$"; break;
        //        case 2: strSymbol = "€"; break;
        //        case 3: strSymbol = "£"; break;
        //        case 4: strSymbol = "₪"; break;
        //    }
        //    return "#,0.00" + strSymbol + ";#,0.00" + strSymbol;
        //}//getSecurityCurrencyFormat

        #endregion General methods

        #region Category Items

        public ICategoryItem getCurrentCategoryValue(enumCatType eType)
        { // Retrieves the desired category item by type
            switch (eType)
            {
                case enumCatType.Sector: return m_catSector;
                case enumCatType.SecurityType: return m_catSecType;
                case enumCatType.StockMarket: return m_catMarket;
                //case enumCatType.Currency: return m_catCurrency;
            }
            return null;
        }//getCurrentCategoryValue

        #endregion Category Items

        #endregion Methods

        #region Properties

        #region Main variables

        public string SecurityName
        {
            get { return m_strSecName; }
            set { m_strSecName = getSecName(value); }
        }//SecurityName

        public string HebName
        {
            get { return m_strHebName; }
            set { m_strHebName = getSecName(value); }
        }//HebName

        public string SecuritySymbol
        { 
            get { return m_strSecSymbol; }
            set { m_strSecSymbol = value; }
        }//SecuritySymbol

        public string SecurityDisplay
        {  get { return getSecDisplayValue(); } }//SecurityDisplay

        public string ISIN
        {
            get { return m_strISIN; }
            set { m_strISIN = value; }
        }//ISIN

        public String PortSecurityId
        {
            get { return m_iPortSecId; }
            set { m_iPortSecId = value; }
        }//PortSecurityId

        public int NumericID
        {
            get { return m_iNumericId; }
            set { m_iNumericId = value; }
        }//NumericID

        #endregion Main variables

        #region Category variables

        public ICategoryItem SecurityType
        {
            get { return m_catSecType; }
            set { m_catSecType = value; }
        }//SecurityType

        public ICategoryItem Sector
        {
            get { return m_catSector; }
            set { m_catSector = value; }
        }//Sector

        public ICategoryItem Market
        {
            get { return m_catMarket; }
            set { m_catMarket = value; }
        }//Market

        public String MarketName
        {
            get { return m_strMarket; }
            set { m_strMarket = value; }
        }//MarketName

        //public cCategoryItem Currency
        //{
        //    get { return m_catCurrency; }
        //    set { m_catCurrency = value; }
        //}//Currency

        public Color SecColor
        {
            get { return m_clrSecColor; }
            set { m_clrSecColor = value; }
        }//SecColor

        #endregion Category variables

        #endregion Properties

    }//of class
}
