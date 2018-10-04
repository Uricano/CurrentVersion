using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

// Used namespaces
using TFI.BusinessLogic.Enums;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.Portfolio;
using System.Web;
using System.Configuration;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.TFI.BusinessLogic.General
{
    public static class cProperties
    {

        #region Data members

        // Application variables
        private static ICollectionsHandler m_objColHandler; // Collection handler global variable
        private static Boolean m_isFirstTime = false; // Whether this is the first time the user runs the software (since the installation)
        private static Boolean m_isWithInternet = true; // Whether the application runs with the internet connection or offline
        private static Boolean m_isWithQA = false; // Whether the user can use QA functions (for developers)
        private static Boolean m_isQARequested = false; // Whether the user has made a request to print the QA results
        private static String m_strQaFilePrefix = ""; // File prefix (for saving QA files, including folder)
        private static Boolean m_is32BitOperating = true; // Whether we work on a 32-bit operating system
        private static Boolean m_isIsraelOnly = false; // Whether the application will display the data in hebrew or not
        private static List<int> m_colAllowedExchanges = new List<int>(); // Allowed exchanges (in license)
        //private static String m_strAppFolder = HttpContext.Current.Server.MapPath("~/App_Data"); // Folder of application
        private static String m_strFilesFolder = HttpContext.Current.Server.MapPath("~/App_Data/Logs"); // Folder for app data
        private static Stopwatch m_objStopWatch = new Stopwatch(); // Stopwatch variable - for measurements
        private static Boolean m_isBenchmarksCalcDone = false; // Whether calculations for Benchmarks collection return rates were performed once

        // Display variables
        private static String m_strDisplayFldName = "strName"; // Name of data field in DB tables
        private static enumSecurityDisplay m_enumSecDisplay = initSecDisplay();    // Aspect of security to display
        //private static String m_strDateSortOrder = Properties.Settings.Default.DateSortOrder;   // Sort order of dates in project
        private static double m_dTPRiskLevel = -20D; // Risk level of TP (for cataloging)

        // Data variables
        private static Boolean m_isAllSecsSelection = true; // Whether the user performs analysis on all securities / selected securities
        private static int m_iUserID = -1; // User ID
        private static IPortfolioBL m_objOpenPort; // current portfolio
        private static int m_iNumOfBondOpts = 0; // Number of bond optimizations left (from license)
        private static int m_iNumOfOptimizations = 0; // Number of optimizations left (from license)
        //private static int m_iDateInterval = int.Parse(ConfigurationManager.AppSettings["DateInterval"]); // Date range interval (in years)
        private static int m_iDateInterval = 5; // Date range interval (in years)
        private static int m_iMinMonthsOfData = 6; // Mimimum of data (in monthes) required for optimization (per security)
        private static DateTime m_dtLastUpdate = DateTime.Today.AddDays(-1); // Last updated prices of the local DB
        private static DateTime m_dtLastOptimization = DateTime.Today; // Last optimization date 
        private static DataTable m_dtDataForQa; // Contains the data to be written in the QA file
        private static int m_iQaDataPos = 0; // Current position in Datatable for QA
        private static int m_iDataPointsThres = 20; // Minimum number of data points (prices) required
        private static double m_dMaxRiskVal = 0.9D; // Maximum risk level allowed for optimization (per security)

        // Calculation variables
        private static enumEfCalculationType m_enOptimizationType = enumEfCalculationType.Custom; // Optimization calculation type
        private static enumEfCalculationType m_enOptimizationDefaultType = enumEfCalculationType.Custom; // Optimization default calculation type
        private static String m_strCurrentCurrencyId = "9001"; // Current currency ID (Conversion from $ to ILS)
        private static enumDateFreq m_enDateFreq = initDateFreq(); // Frequency of data
        private static double m_dLastDollarVal = -1; // Last dollar value

        // General variables
        private static String m_strProjectCode = "AMAAMACpngCx+8b99VD8FJwbwobSFYGIzRd27QWAuIWyoRkeaZfLZstkXNrREJmp+N6boJUDAAEAAQ=="; // Validation key
        private static Random m_rndMainGenerator = new Random(); // Random generator for entire project
        private static String m_strUserName = ""; // User name
        private static String m_strUserPassword = ""; // User password
        private static string _sendMailTo1 = "uriel@takin.co.il";
        private static string _sendMailTo2 = "merav@takin.co.il";
        private static string m_securityCode = "";
        // Constants
        private static String m_cnstPortfolioName = "CherriesPortfolio"; // Cherries default portfolio name (non modifiable)
        private static String m_cnstClosePriceFldName = "fClose"; // Close price field name
        private static String m_cnstAdjPriceFldName = "dAdjPrice"; // Adjusted Close price field name (for tsua calculations)
        private const String m_cnstRateFldName = "dRateVal"; // Rates field name
        private const String m_cnstEncryptionPass = "tak_instruments"; // Password for our encryptions
        private const int m_iDailyFreqScaling = 252; // Daily frequency scaling (for rate + covar calculations)
        private const int m_iWeeklyFreqScaling = 52; // Weekly frequency scaling (for rate + covar calculations)
        private const int m_cnstBenchmarkInd = 106; // Index of benchmark security type
        private const int m_iOptimalPorts = 100; // Number of optimal portfolios in calculations
        private const int m_idTaExchange = 1; // TASE id

        private static Boolean m_isWithCash = true;       // Whether CASH is calculated
        private static Boolean m_runAutoOptimization = false; // Whether there are no portfolios in local database (actualy like running CHERRIES for the first time)

        #endregion Data members

        #region Methods

        #region Init Methods

        private static enumDateFreq initDateFreq()
        {//init date frequency
            
            return enumDateFreq.Weekly;
        }//initDateFreq

        public static int getCurrentScaling()
        { // Retrieves current frequency scaling
            switch (m_enDateFreq)
            {
                case enumDateFreq.Daily: return m_iDailyFreqScaling;
                default: return m_iWeeklyFreqScaling;
            }
        }//getCurrentScaling

        private static enumSecurityDisplay initSecDisplay()
        {//init sec display
            
            return enumSecurityDisplay.Ticker;
        }//initSecDisplay

        #endregion Init Methods

        #region Save Settings Methods

        //public static Boolean saveSecDisplay(int iSecDisplay)
        //{//save sec display
        //    if (iSecDisplay >= 1 && iSecDisplay <= 2)
        //    {
        //        Properties.Settings.Default.SecDisplay = iSecDisplay;
        //        Properties.Settings.Default.Save();
        //        return true;
        //    }
        //    return false;
        //}//saveSecDisplay

        //public static Boolean saveDateSortOrder(String sOrder)
        //{//save date sort order
        //    if (sOrder == "DESC" || sOrder == "ASC")
        //    {
        //        Properties.Settings.Default.DateSortOrder = sOrder;
        //        Properties.Settings.Default.Save();
        //        return true;
        //    }
        //    return false;
        //}//saveDateSortOrder

        //public static Boolean savePriceType(int iPriceType)
        //{//save price type
        //    if (iPriceType >= 1 && iPriceType <= 3)
        //    {
        //        Properties.Settings.Default.PriceType = iPriceType;
        //        Properties.Settings.Default.Save();
        //        return true;
        //    }
        //    return false;
        //}//savePriceType

        //public static Boolean saveDateFreq(int iDateFreq)
        //{//save date Frequency
        //    if (iDateFreq >= 1 && iDateFreq <= 2)
        //    {
        //        Properties.Settings.Default.DateFreq = iDateFreq;
        //        Properties.Settings.Default.Save();
        //        return true;
        //    }
        //    return false;
        //}//saveDateFreq

        //public static Boolean saveDevPercent(double dDevPercent)
        //{//save Deviation Percent
        //    Properties.Settings.Default.DeviationPercent = dDevPercent;
        //    Properties.Settings.Default.Save();
        //    return true;
        //}//saveDevPercent

        #endregion Save Settings Methods

        #endregion Methods

        #region Properties

        #region Application variables

        public static ICollectionsHandler CollectionHandler
        {
            get { return m_objColHandler; }
            set { m_objColHandler = value; }
        }//CollectionHandler

        public static Boolean isFirstTime
        {
            get { return m_isFirstTime; }
            set { m_isFirstTime = value; }
        }//isFirstTime
        public static Boolean isWithInternet
        {
            get { return m_isWithInternet; }
            set { m_isWithInternet = value; }
        }//isWithInternet

        public static Boolean runAutoOptimization
        {
            get { return m_runAutoOptimization; }
            set { m_runAutoOptimization = value; }
        }//runAutoOptimization

        public static Boolean isBenchmarksCalcDone
        {
            get { return m_isBenchmarksCalcDone; }
            set { m_isBenchmarksCalcDone = value; }
        }//isBenchmarksCalcDone

        public static Boolean isWithQA
        {
            get { return m_isWithQA; }
            set { m_isWithQA = value; }
        }//isWithQA

        public static Boolean isQARequested
        {
            get { return m_isQARequested; }
            set { m_isQARequested = value; }
        }//isQARequested

        public static String qaFilePrefix
        {
            get { return m_strQaFilePrefix; }
            set { m_strQaFilePrefix = value; }
        }//qaFilePrefix

        public static Boolean is32Bit
        {
            get { return m_is32BitOperating; }
            set { m_is32BitOperating = value; }
        }//is32Bit

        //public static Boolean isWithBondOpt
        //{
        //    get { return m_isWithBondOpt; }
        //    set { m_isWithBondOpt = value; }
        //}//isWithBondOpt

        //public static Boolean isLicActivated
        //{
        //    get { return m_isLicActivated; }
        //    set { m_isLicActivated = value; }
        //}//isLicActivated

        public static Boolean isIsraelOnly
        {
            get { return m_isIsraelOnly; }
            set { m_isIsraelOnly = value; }
        }//isIsraelOnly

        public static List<int> AllowedExchanges
        {
            get { return m_colAllowedExchanges; }
            set { m_colAllowedExchanges = value; }
        }//AllowedExchanges

        
        public static String DataFolder
        {
            get { return m_strFilesFolder; }
            set { m_strFilesFolder = value; }
        }//SaveFolder

        public static Stopwatch Watch
        {
            get { return m_objStopWatch; }
            set { m_objStopWatch = value; }
        }//Watch

        #endregion Application variables

        #region Display variables

        public static enumSecurityDisplay DisplayOfSecurity
        {
            get { return m_enumSecDisplay; }
            set { m_enumSecDisplay = value; }
        }//DisplayOfSecurity

        public static String DisplayFieldName
        {
            get { return m_strDisplayFldName; }
            set { m_strDisplayFldName = value; }
        }//DisplayFieldName

       
        public static double TPRisk
        { get { return m_dTPRiskLevel; } }//TPRisk

        #endregion Display variables

        #region Data variables

        public static Boolean AllSecurities
        {
            get { return m_isAllSecsSelection; }
            set { m_isAllSecsSelection = value; }
        }//AllSecurities

        public static int UserID
        {
            get { return m_iUserID; }
            set { m_iUserID = value; }
        }//UserID

        public static IPortfolioBL CurrPortfolio
        {
            get { return m_objOpenPort; }
            set { m_objOpenPort = value; }
        }//CurrPortfolio

        public static int NumBondOpts
        {
            get { return m_iNumOfBondOpts; }
            set { m_iNumOfBondOpts = value; }
        }//NumBondOpts

        public static int NumOptimizations
        {
            get { return m_iNumOfOptimizations; }
            set { m_iNumOfOptimizations = value; }
        }//NumOptimizations

        public static int DatesInterval
        { get { return m_iDateInterval; } }//DatesInterval

        public static int MinMonthsData
        {
            get { return m_iMinMonthsOfData; }
            set { m_iMinMonthsOfData = value; }
        }//MinMonthsData

        public static DateTime LastUpdate
        {
            get { return m_dtLastUpdate; }
            set { m_dtLastUpdate = value; }
        }//LastUpdate

        public static DateTime LastOptimization
        {
            get { return m_dtLastOptimization; }
            set { m_dtLastOptimization = value; }
        }//LastOptimization

        public static DataTable DataForQa
        {
            get { return m_dtDataForQa; }
            set { m_dtDataForQa = value; }
        }//DataForQa

        public static int QaPos
        {
            get { return m_iQaDataPos; }
            set { m_iQaDataPos = value; }
        }//QaPos

        public static int DataPointsThres
        { get { return m_iDataPointsThres; } }//DataPointsThres

        public static double maxRiskVal
        {
            get { return m_dMaxRiskVal; }
            set { m_dMaxRiskVal = value; }
        }//maxRiskVal

        #endregion Data variables

        #region Calculation variables

        public static enumEfCalculationType DefaultOptimizationType
        {
            get { return m_enOptimizationDefaultType; }
            set { m_enOptimizationDefaultType = value; }
        }//DefaultOptimizationType

        public static enumEfCalculationType OptimizationType
        {
            get { return m_enOptimizationType; }
            set { m_enOptimizationType = value; }
        }//OptimizationType

        public static String CurrencyId
        {
            get { return m_strCurrentCurrencyId; }
            set { m_strCurrentCurrencyId = value; }
        }//CurrencyId

        public static enumDateFreq Frequency
        {
            get { return m_enDateFreq; }
            set { m_enDateFreq = value; }
        }//Frequency

        //public static enumFinalRateValue RateFinalType
        //{
        //    get { return m_enRateFinalType; }
        //    set { m_enRateFinalType = value; }
        //}//RateFinalType

        public static double LastDollarVal
        {
            get { return m_dLastDollarVal; }
            set { m_dLastDollarVal = value; }
        }//LastDollarVal

        #endregion Calculation variables

        #region General variables

        public static String ProjectKey
        { get { return m_strProjectCode; } }//ProjectKey

        public static Random RndGenerator
        { get { return m_rndMainGenerator; } }//RndGenerator

        public static String UserName
        {
            get { return m_strUserName; }
            set { m_strUserName = value; }
        }//UserName

        public static String UserPassword
        {
            get { return m_strUserPassword; }
            set { m_strUserPassword = value; }
        }//UserPassword

        public static string sendMailTo1 { get { return _sendMailTo1; } }
        public static string sendMailTo2 { get { return _sendMailTo2; } }
        //public static string sendMailFrom { get { return _sendMailFrom; } }

        #endregion General variables

        #region Constants

        public static String PortfolioName
        { get { return m_cnstPortfolioName; } }//PortfolioName

        //public static String PricesFld
        //{
        //    get { return m_objOpenPort.PricesFld; }
        //    set { m_objOpenPort.PricesFld = value; }
        //}//PricesFld

        public static String ClosePriceFld
        {
            get { return m_cnstClosePriceFldName; }
            set { m_cnstClosePriceFldName = value; }
        }//PricesFld

        public static String AdjPriceFld
        {
            get { return m_cnstAdjPriceFldName; }
            set { m_cnstAdjPriceFldName = value; }
        }//PricesFld

        public static String RatesFld
        { get { return m_cnstRateFldName; } }//RatesFld

        public static String EncryptionPass
        { get { return m_cnstEncryptionPass; } }//EncryptionPass

        public static int DailyScaling
        { get { return m_iDailyFreqScaling; } }//DailyScaling

        public static int WeeklyScaling
        { get { return m_iWeeklyFreqScaling; } }//WeeklyScaling

        public static int BenchmarkID
        { get { return m_cnstBenchmarkInd; } }//BenchmarkID

        public static int TaseID
        { get { return m_idTaExchange; } }//TaseID

        public static int OptimalPorts
        { get { return m_iOptimalPorts; } }//OptimalPorts
        public static Boolean IsWithCash
        { get { return m_isWithCash; } }   // 

        #endregion Constants

        #endregion Properties

    }//of class
}
