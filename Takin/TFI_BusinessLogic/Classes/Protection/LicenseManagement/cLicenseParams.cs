using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Used namespaces
using Cherries.TFI_BusinessLogic.General;

namespace Cherries.TFI_BusinessLogic.Protection.LicenseManagement
{
    public static class cLicenseParams
    {

        #region Data members

        private static int m_iDaysLeft = 0; // Days remaining to expiration
        public static int DaysLeft { get { return m_iDaysLeft; } set { m_iDaysLeft = value; } }
        private static int m_iPortfolioNum = 0; // Number of portfolios
        public static int PortfoliosNum { get { return m_iPortfolioNum; } set { m_iPortfolioNum = value; } }
        private static int m_iExtPortsNum = 0; // External portfolios number
        public static int ExtPortsNum { get { return m_iExtPortsNum; } set { m_iExtPortsNum = value; } }
        private static int m_iSecsNum = 0; // Number of Securities
        public static int SecuritiesNum { get { return m_iSecsNum; } set { m_iSecsNum = value; } }
        private static int m_iOptimizationNum = 0; // Number of optimizations
        public static int OptimizationNum { get { return m_iOptimizationNum; } set { m_iOptimizationNum = value; } }
        private static int m_iFollowupNum = 20; // Allowed Followup portfolios
        public static int FollowupNum { get { return m_iFollowupNum; } set { m_iFollowupNum = value; } }
        private static int m_iBondOptNum = 0; // Number of bond optimizations
        public static int BondOptNum { get { return m_iBondOptNum; } set { m_iBondOptNum = value; } }

        private static Boolean m_isAllowQA = false; // Allows QA testing functions
        public static Boolean AllowQA { get { return m_isAllowQA; } set { m_isAllowQA = value; } }
        private static Boolean m_isPrivateUser = true; // Distincts private / commercial user
        public static Boolean isPrivate { get { return m_isPrivateUser; } set { m_isPrivateUser = value; } }
        //private static Boolean m_isIsraelOnly = false; // Displays data in hebrew
        //public static Boolean isIsraelOnly { get { return m_isIsraelOnly; } set { m_isIsraelOnly = value; } }
        private static Boolean m_isTrial = false; // Whether this is a trial version or not
        public static Boolean isTrial { get { return m_isTrial; } set { m_isTrial = value; } }
        private static String m_strExchanges = "1, 5"; // Exchanges allowed in license
        public static String Exchanges { get { return m_strExchanges; } set { m_strExchanges = value; } }

        //private static DateTime m_dtMaxDate = DateTime.MinValue; // Maximal date to which you can update
        //public static DateTime MaxDate { get { return m_dtMaxDate; } set { m_dtMaxDate = value; } }

        private static DateTime m_dtDateExpires = DateTime.Today.AddDays(3); // Date in which the license expires
        public static DateTime DateExpires { get { return m_dtDateExpires; } set { m_dtDateExpires = value; } }

        private static String m_strCompID = ""; // Computer's ID
        public static String CompID { get { return m_strCompID; } set { m_strCompID = value; } }

        #endregion Data members

        #region Methods

        public static void setUserData(String strUserData)
        { // Seperates userData received from license and sets the proper project variables
            if (strUserData == "") return;
            System.Data.Common.DbConnectionStringBuilder sb = new System.Data.Common.DbConnectionStringBuilder();
            sb.ConnectionString = strUserData;

            m_iPortfolioNum = Convert.ToInt32(sb["PortfolioQnt"].ToString());
            m_iExtPortsNum = Convert.ToInt32(sb["ExtPortsQnt"].ToString());
            m_iSecsNum = Convert.ToInt32(sb["SecPerPortfolio"].ToString());
            m_iOptimizationNum = Convert.ToInt32(sb["OptimizationQnt"].ToString());
            m_iBondOptNum = Convert.ToInt32(sb["BondOptimizationQnt"].ToString());

            m_isPrivateUser = Convert.ToBoolean(sb["isPrivateUser"].ToString());
            m_isAllowQA = Convert.ToBoolean(sb["isQA"].ToString());
            //m_isIsraelOnly = Convert.ToBoolean(sb["isHebrewOnly"].ToString());
            m_isTrial = Convert.ToBoolean(sb["isTrial"].ToString());
            try { m_strExchanges = sb["Exchanges"].ToString(); }
            catch { }
            //m_dtMaxDate = DateTime.ParseExact(sb["MaxBringUpToDate"].ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            //if (m_dtMaxDate < cProperties.MaxDate) cProperties.MaxDate = m_dtMaxDate;
        }//setUserData

        public static void clearUserData()
        { // Clears the user data (reboot license parameters)
            m_iPortfolioNum = 0;
            m_iExtPortsNum = 0;
            m_iSecsNum = 0;
            m_isAllowQA = false;
            //m_isIsraelOnly = false;
            m_isTrial = false;
        }//clearUserData

        public static String getPackageName(String strExchanges)
        { // Retrieves the name of the package from the exchanges
            string[] arrExchanges = strExchanges.Split(',');
            if (arrExchanges.Length == 1) return "Tel-Aviv Stock Exchange (TASE)";
            if (arrExchanges.Length == 4) return "Israel + USA (Premium package)";
            return "USA (AMEX, NYSE, NASDAQ)";
        }//getPackageName

        #endregion Methods

    }//of class
}
