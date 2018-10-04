using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.TFI.BusinessLogic.Protection.LicenseManagement
{
    public class cShoppingParams
    {
        // CLIENT PARAMETERS
        private static cClientParams m_objClientParams; // Client parameters variable
        public static cClientParams ClientParams
        { get { return m_objClientParams; } set { m_objClientParams = value; } }

        // RECEIPT PARAMETERS
        private static cReceiptParams m_objReceiptParams; // Client parameters variable
        public static cReceiptParams ReceiptParams
        { get { return m_objReceiptParams; } set { m_objReceiptParams = value; } }

        // LICENSE PARAMETERS
        private static cPurchasedLicParams m_objLicParams; // Client parameters variable
        public static cPurchasedLicParams LicenseParams
        { get { return m_objLicParams; } set { m_objLicParams = value; } }

    }//of class

    #region Sub classes

    public class cClientParams
    {
        // FULL NAME
        private static String m_strFullName = ""; // Full name variable
        public static String FullName
        { get { return m_strFullName; } set { m_strFullName = value; } }
        // E-MAIL
        private static String m_strEMail = ""; // e-Mail variable
        public static String eMail
        { get { return m_strEMail; } set { m_strEMail = value; } }
        // PHONE
        private static String m_strPhone = ""; // Phone variable
        public static String Phone
        { get { return m_strPhone; } set { m_strPhone = value; } }
    }//class (cClientParams)

    public class cReceiptParams
    {
        // RECEIPT
        private static String m_strReceipt = ""; // Receipt variable
        public static String ReceiptCode
        { get { return m_strReceipt; } set { m_strReceipt = value; } }
        // AMOUNT
        private static double m_dAmountPaid = 0D; // Amount of money payed
        public static double Amount
        { get { return m_dAmountPaid; } set { m_dAmountPaid = value; } }
    }//class (cClientParams)

    public class cPurchasedLicParams
    {
        // POOLS
        private static int m_iPools = 0; // Pools variable
        public static int Pools
        { get { return m_iPools; } set { m_iPools = value; } }
        // SEC PER POOL
        private static int m_iSecsPerPool = 0; // Secs per pool variable
        public static int SecsPerPool
        { get { return m_iSecsPerPool; } set { m_iSecsPerPool = value; } }
        // EXTERNAL PORTFOLIOS
        private static int m_iExtPorts = 0; // External portfolios variable
        public static int ExtPorts
        { get { return m_iExtPorts; } set { m_iExtPorts = value; } }
        // STOCK MARKETS
        private static int m_iExchanges = 0; // Exchanges variable
        public static int Exchanges
        { get { return m_iExchanges; } set { m_iExchanges = value; } }
        // PERIOD
        private static int m_iPeriod = 0; // Period (days) variable
        public static int Period
        { get { return m_iPeriod; } set { m_iPeriod = value; } }
        // USE BOND OPT
        private static int m_iBondOpt = 0; // Bond optimization variable
        public static int BondOpt
        { get { return m_iBondOpt; } set { m_iBondOpt = value; } }
        // LICENSE QUANTITY
        private static int m_iLicQuantity = 0; // License quantity variable
        public static int Quantity
        { get { return m_iLicQuantity; } set { m_iLicQuantity = value; } }
    }//class (cClientParams)

    #endregion Sub classes

}//namespace
