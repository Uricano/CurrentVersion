using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CherriesBatchProcess
{
    public class Constants
    {
        // processes names and ids
        public static string EODSecuritiesImport = "EODSecuritiesImport";
        public static string EODSecuritiesImportProcessId = "1";
        public static string EODPricesImport = "EODPricesImport";
        public static string EODPricesImportProcessId = "2";
        public static string PredictaImport = "PredictaImport";
        public static string PredictaImportProcessId = "4";
        public static string EODHistPricesImport = "EODHistPricesImport";
        public static string EODHistPricesImportProcessId = "12";
        public static string PredictaHistImport = "PredictaHistImport";
        public static string PredictaHistImportProcessId = "14";
        public static string BatchCalculation = "BatchCalculation";
        public static string BatchCalculationProcessId = "20";
        public static string UpdateCherries = "UpdateCherries";
        public static string UpdateCherriesProcessId = "24";

        // Quandl
        public static string QuandlSecuritiesImport = "QuandlSecuritiesImport";
        public static string QuandlSecuritiesImportProcessId = "6";
        public static string QuandlPricesImport = "QuandlPricesImport";
        public static string QuandlPricesImportProcessId = "8";
        
        // Intrinio
        public static string IntrinioSecuritiesImport = "IntrinioSecuritiesImport";
        public static string IntrinioSecuritiesImportProcessId = "16";
        public static string IntrinioPricesImport = "IntrinioPricesImport";
        public static string IntrinioPricesImportProcessId = "18";
        public static string IntrinioBatchCalculation = "IntrinioBatchCalculation";
        public static string IntrinioBatchCalculationProcessId = "26";

        // config file properties
        public static string DB_ConnectionString = "DB_ConnectionString";
        public static string DB_ConnectionString_Quandl = "DB_ConnectionString_Quandl";
        public static string SourceDir = "SourceDir";
        public static string ProcessedDir = "ProcessedDir";
        public static string RejectedDir = "RejectedDir";
        public static string TempDir = "TempDir";

        // **** Intrinio ****
        // Securities
        public static string IntrinioAPIkey = "IntrinioAPIkey";
        public static string IntrinioSecuritiesInitFTPServerURL = "IntrinioSecuritiesInitFTPServerURL";
        public static string IntrinioSecuritiesInitFTPServerURL_TASE = "IntrinioSecuritiesInitFTPServerURL_TASE";
        public static string IntrinioSecuritiesFTPServerURL_start = "IntrinioSecuritiesFTPServerURL_start";
        public static string IntrinioSecuritiesFTPServerURL_end = "IntrinioSecuritiesFTPServerURL_end";
        public static string IntrinioSecuritiesFTPUserName = "IntrinioSecuritiesFTPUserName";   //"EODSecuritiesFTPUserName"; it worked with commented out values - why???????
        public static string IntrinioSecuritiesFTPPassword = "IntrinioSecuritiesFTPPassword";   //"EODSecuritiesFTPPassword";
        // Prices
        public static string IntrinioHistPricesFTPServerURL = "IntrinioHistPricesFTPServerURL";
        public static string IntrinioPricesFTPServerURL = "IntrinioPricesFTPServerURL";
        public static string IntrinioPricesFTPUserName = "IntrinioSecuritiesFTPUserName";       // it is the same password, so for now not creating another attribute.
        public static string IntrinioPricesFTPPassword = "IntrinioSecuritiesFTPPassword";
        // *****************

        // ***** QUANDL ****
        // Securities
        public static string QuandlAPIkey = "QuandlAPIkey";
        public static string QuandlSecuritiesFTPServerURL = "QuandlSecuritiesFTPServerURL";
        public static string QuandlSecuritiesFTPUserName = "QuandlSecuritiesFTPUserName";   //"EODSecuritiesFTPUserName"; it worked with commented out values - why???????
        public static string QuandlSecuritiesFTPPassword = "QuandlSecuritiesFTPPassword";   //"EODSecuritiesFTPPassword";
        // Prices
        public static string QuandlHistPricesFTPServerURL = "QuandlHistPricesFTPServerURL";
        public static string QuandlPricesFTPServerURL = "QuandlPricesFTPServerURL";
        public static string QuandlPricesFTPUserName = "QuandlSecuritiesFTPUserName";       // it is the same password, so for now not creating another attribute.
        public static string QuandlPricesFTPPassword = "QuandlSecuritiesFTPPassword";
        // *****************

        public static string EODPricesFTPServerURL = "EODPricesFTPServerURL";
        public static string EODSecuritiesFTPServerURL = "EODSecuritiesFTPServerURL";
        public static string EODPricesFTPUserName = "EODPricesFTPUserName";
        public static string EODSecuritiesFTPUserName = "EODSecuritiesFTPUserName";
        public static string EODPricesFTPPassword = "EODPricesFTPPassword";
        public static string EODSecuritiesFTPPassword = "EODSecuritiesFTPPassword";
        public static string PredictaFTPServerURL = "PredictaFTPServerURL";
        public static string PredictaFTPUserName = "PredictaFTPUserName";
        public static string PredictaFTPPassword = "PredictaFTPPassword";
        public static string Exchanges = "Exchanges";
        public static string EODExchanges = "EODExchanges";
        public static string QuandlExchanges = "QuandlExchanges";
        public static string IntrinioExchanges = "IntrinioExchanges";
        public static string ChooseSecuritiesForExchange = "ChooseSecuritiesForExchange";
        public static string SchedulerSecondsToSleep = "SchedulerSecondsToSleep";
        public static string DoDeleteFiles = "DeleteFiles";

        // parameters values
        public static string Import = "-import";
        public static string EOD = "eod";
        public static string Predicta = "predicta";
        public static string Quandl = "quandl";
        public static string Intrinio = "intrinio";
        public static string Calc = "-calc";
        public static string CalcIntrinio = "-calcintrinio";
        public static string Scheduler = "-scheduler";
        public static string Last = "-last";
        public static string Daily = "-daily";
        public static string Initial = "-initial";  // now for first time security import only
        public static string Prices = "-p";
        public static string Securities = "-s";
        public static string Indeces = "-i";
        public static string HistPrices = "-h";

        // date parameters format
        public static string DateParmsFormat = "yyyy/MM/dd";

        // stored procedures for EOD prices and indeces rates import
        public static string importEODCleanDataForEODImport = "importEODCleanDataForEODImport";  // LR changed - wrong tbl_Securities - "importEODCleanDataForEODImport";
        public static string importCleanDataForEODIndexesImport = "importCleanDataForEODIndexesImport";
        public static string importEODImport = "importEODImport";
        public static string importEODIndexes = "importEODIndexes";

        // stored procedures for EOD securities import
        public static string importEODSecuritiesImport = "importEODSecuritiesImport";

        // stored procedures for QUANDL securities import
        public static string importQuandlSecurities = "importQuandlSecurities";
        public static string importQuandlPrices = "importQuandlPrices";
        public static string importQuandlHistPrices = "importQuandlHistoricalPrices";


        // stored procedures for INTRINIO import
        public static string importIntrinioInitSecurities = "importIntrinioInitSecurities"; //***importIntrinioSecurities
        public static string importIntrinioSecurities = "importIntrinioSecurities";         //***importIntrinioSecurities
        public static string importCleanDataPriorPricesImport = "importCleanDataPriorPricesImport";
        public static string importIntrinioPrices = "importIntrinioPrices";
        public static string importIntrinioHistIndicesPrices = "importIntrinioHistIndicesPrices";

        // stored procedures for Predicta import
        public static string importPredictaSecuritiesImport = "importPredictaSecuritiesImport";
        public static string importPredictaPricesImport = "importPredictaPricesImport";
        public static string importPredictaIndexRatesImport = "importPredictaIndexRatesImport";

        // stored procedures for batch calculations
        public static string ShekelLeShekel = "dataShekelLeShekel";
        public static string CompleteMissingPrices = "dataCompleteMissingPrices";
        public static string CompleteMissingIndexPrices = "dataCompleteMissingIndexPrices";
        public static string SetIsTraded = "dataSetIsTraded";
        public static string SetDuplicateSymbols = "dataSetDuplicateSecToNonTraded";
        public static string UpdtPricesValidity = "dataUpdtPricesValidity";
        public static string UpdtIndexRatesValidity = "dataUpdtIndexRatesValidity";
        public static string DuplicateLastDayPrices = "dataDuplicateLastDayPrices";
        public static string DuplicateLastDayIndexPrices = "dataDuplicateLastDayIndexPrices";
        public static string PriceReturns = "calcPriceReturns";
        public static string PriceReturnsWeekly = "calcWeeklyPriceReturns";
        public static string AvgYield = "calcAvgYield";
        public static string StdYield = "calcStdYield";
        public static string MonetaryAvg = "calcMonetaryAvg";
        public static string SecurityRanking = "calcSetSecuritiesRanking"; // LR changed name -"calcSecuritiesRanking";
        public static string SecurityCalcHistory = "calcSecuritiesHistory";

        // filename formats
        public static string EODDateInFilenameFormat = "yyyyMMdd";
        public static string PredictaFilenamePrefix = "applause_";
        public static string PredictaSecuritiesFilename = "ApplauseSecurityList.xml";
        public static string PredictaPricesFilename = "ApplauseSecurityTotal.xml";
        public static string PredictaIndecesFilename = "ApplauseIndexTotal.xml";
        public static string PredictaDateInFilenameFormat = "ddMMyyyy";
        public static int PredictaFilenameLength = 21;
        public static string EODIndecesImportFilenamePrefix = "INDEX";

        // special exchange codes
        public static int IndecesExchangeCode = -2;
        public static int PredictaExchangeId = 1;
        public static string PredictaExchange = "TASE";
        
        // logger codes
        public static string Success = "S";
        public static string Error = "E";
        public static string Failure = "F";

        // timeout for stored procedures in seconds
        public static int SPsTimeout = 28800;

    }
}
