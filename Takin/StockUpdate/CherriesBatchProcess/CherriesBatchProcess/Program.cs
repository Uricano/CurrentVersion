/**
 * CherriesBatchProcess -import predicta PredictaImport_parameters_dates_interval_parameters [-s] [-p] [-i]
 *   securities, prices and/or index rates
 * 
 * CherriesBatchProcess -import eod -s
 *   securities import
 * 
 * CherriesBatchProcess -import eod EODPricesImport_dates_interval_parameters -p
 *   prices import
 *   
 * CherriesBatchProcess -import eod EODPricesImport_dates_interval_parameters -i
 *   indeces rates import
 *   
 * CherriesBatchProcess -calc
 *   SP calculation process
 *   
 * 
 * in these modes we call import/calculations from command line. 
 * EOD prices/securities import and calculation process takes exchanges to process list from configuration.
 * Calculation process SP calcSecuritiesRanking choose for each exchange number of securities specified in configuration. 
 * EOD indeces import work with file prefix='INDEX' and exchange code -2 that specified in Constants class. 
 * 
 * 
 * CherriesBatchProcess -scheduler
 * 
 * Sleeps and raise each number of seconds specified in configuration.
 * Works according to exchanges specified in tbl_ExchangeTiming table.
 * 2 special cases: exchangeId=1 - Predicta import and exchangeId=-2 - EOD indeces import.
 * After import runs SP calculation process.
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.IO;

namespace CherriesBatchProcess
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Dictionary<string, string> config = new Dictionary<string, string>();
            var path = Environment.CurrentDirectory;
            config[Constants.DB_ConnectionString] = Convert.ToString(ConfigurationManager.ConnectionStrings[Constants.DB_ConnectionString]);
            config[Constants.DB_ConnectionString_Quandl] = Convert.ToString(ConfigurationManager.ConnectionStrings[Constants.DB_ConnectionString_Quandl]);
            config[Constants.SourceDir] = path +ConfigurationManager.AppSettings[Constants.SourceDir];
            config[Constants.ProcessedDir] = path +ConfigurationManager.AppSettings[Constants.ProcessedDir];
            config[Constants.RejectedDir] = path + ConfigurationManager.AppSettings[Constants.RejectedDir];
            config[Constants.TempDir] = path +ConfigurationManager.AppSettings[Constants.TempDir];
            config[Constants.EODPricesFTPServerURL] = ConfigurationManager.AppSettings[Constants.EODPricesFTPServerURL];
            config[Constants.EODPricesFTPUserName] = ConfigurationManager.AppSettings[Constants.EODPricesFTPUserName];
            config[Constants.EODPricesFTPPassword] = ConfigurationManager.AppSettings[Constants.EODPricesFTPPassword];
            config[Constants.EODSecuritiesFTPServerURL] = ConfigurationManager.AppSettings[Constants.EODSecuritiesFTPServerURL];
            config[Constants.EODSecuritiesFTPUserName] = ConfigurationManager.AppSettings[Constants.EODSecuritiesFTPUserName];
            config[Constants.EODSecuritiesFTPPassword] = ConfigurationManager.AppSettings[Constants.EODSecuritiesFTPPassword];
            config[Constants.PredictaFTPServerURL] = ConfigurationManager.AppSettings[Constants.PredictaFTPServerURL];
            config[Constants.PredictaFTPUserName] = ConfigurationManager.AppSettings[Constants.PredictaFTPUserName];
            config[Constants.PredictaFTPPassword] = ConfigurationManager.AppSettings[Constants.PredictaFTPPassword];
            config[Constants.Exchanges] = ConfigurationManager.AppSettings[Constants.Exchanges];
            config[Constants.EODExchanges] = ConfigurationManager.AppSettings[Constants.EODExchanges];
            config[Constants.QuandlExchanges] = ConfigurationManager.AppSettings[Constants.QuandlExchanges];
            config[Constants.IntrinioExchanges] = ConfigurationManager.AppSettings[Constants.IntrinioExchanges];
            config[Constants.ChooseSecuritiesForExchange] = ConfigurationManager.AppSettings[Constants.ChooseSecuritiesForExchange];
            config[Constants.SchedulerSecondsToSleep] = ConfigurationManager.AppSettings[Constants.SchedulerSecondsToSleep];
            config[Constants.DoDeleteFiles] = ConfigurationManager.AppSettings[Constants.DoDeleteFiles];

            // Quandl securities
            config[Constants.QuandlAPIkey] = ConfigurationManager.AppSettings[Constants.QuandlAPIkey];
            config[Constants.QuandlSecuritiesFTPServerURL] = ConfigurationManager.AppSettings[Constants.QuandlSecuritiesFTPServerURL];
            config[Constants.QuandlSecuritiesFTPUserName] = ConfigurationManager.AppSettings[Constants.QuandlSecuritiesFTPUserName];
            config[Constants.QuandlSecuritiesFTPPassword] = ConfigurationManager.AppSettings[Constants.QuandlSecuritiesFTPPassword];

            // Quandl prices
            config[Constants.QuandlHistPricesFTPServerURL] = ConfigurationManager.AppSettings[Constants.QuandlHistPricesFTPServerURL];
            config[Constants.QuandlPricesFTPServerURL] = ConfigurationManager.AppSettings[Constants.QuandlPricesFTPServerURL];
            config[Constants.QuandlPricesFTPUserName] = ConfigurationManager.AppSettings[Constants.QuandlPricesFTPUserName];
            config[Constants.QuandlPricesFTPPassword] = ConfigurationManager.AppSettings[Constants.QuandlPricesFTPPassword];

            // Intrinio securities
            config[Constants.IntrinioAPIkey] = ConfigurationManager.AppSettings[Constants.IntrinioAPIkey];
            config[Constants.IntrinioSecuritiesInitFTPServerURL] = ConfigurationManager.AppSettings[Constants.IntrinioSecuritiesInitFTPServerURL];
            config[Constants.IntrinioSecuritiesInitFTPServerURL_TASE] = ConfigurationManager.AppSettings[Constants.IntrinioSecuritiesInitFTPServerURL_TASE];
            config[Constants.IntrinioSecuritiesFTPServerURL_start] = ConfigurationManager.AppSettings[Constants.IntrinioSecuritiesFTPServerURL_start];
            config[Constants.IntrinioSecuritiesFTPServerURL_end] = ConfigurationManager.AppSettings[Constants.IntrinioSecuritiesFTPServerURL_end];
            config[Constants.IntrinioSecuritiesFTPUserName] = ConfigurationManager.AppSettings[Constants.IntrinioSecuritiesFTPUserName];
            config[Constants.IntrinioSecuritiesFTPPassword] = ConfigurationManager.AppSettings[Constants.IntrinioSecuritiesFTPPassword];

            // Intrinio prices
            config[Constants.IntrinioHistPricesFTPServerURL] = ConfigurationManager.AppSettings[Constants.IntrinioHistPricesFTPServerURL];
            config[Constants.IntrinioPricesFTPServerURL] = ConfigurationManager.AppSettings[Constants.IntrinioPricesFTPServerURL];
            config[Constants.IntrinioPricesFTPUserName] = ConfigurationManager.AppSettings[Constants.IntrinioPricesFTPUserName];
            config[Constants.IntrinioPricesFTPPassword] = ConfigurationManager.AppSettings[Constants.IntrinioPricesFTPPassword];



            string[] eodExchanges = null;
            try
            {
                eodExchanges = config[Constants.EODExchanges].Split(new char[] { ',' });
            }
            catch
            {
            }

            string[] exchanges = null;
            try
            {
                exchanges = config[Constants.Exchanges].Split(new char[] { ',' });
            }
            catch
            {
            }
            if (!Directory.Exists(config[Constants.SourceDir])) Directory.CreateDirectory(config[Constants.SourceDir]);
            if (!Directory.Exists(config[Constants.ProcessedDir])) Directory.CreateDirectory(config[Constants.ProcessedDir]);
            if (!Directory.Exists(config[Constants.RejectedDir])) Directory.CreateDirectory(config[Constants.RejectedDir]);
            if (!Directory.Exists(config[Constants.TempDir])) Directory.CreateDirectory(config[Constants.TempDir]);
            //LR : added to run from the source code
            //args = new string[] { "-import", "eod", "2017/07/01", "2017/07/31", "-i" };
            //args = new string[] { "-import", "eod", "2017/08/17", "2017/08/23", "-p" };
            //args = new string[] { "-import", "eod", "-daily", "-s" };
            //args = new string[] { "-import", "eod", "-daily", "-i" };
            //args = new string[] { "-import", "eod", "-daily", "-i" };
            //args = new string[] { "-import", "eod", "-last", "-i" };
            //args = new string[] { "-calc" };
            //args = new string[] { "-import", "eod", "-s" };
            //args = new string[] { "-import", "predicta", "-daily", "-s" };
            //args = new string[] { "-import", "predicta", "2017/08/03", "2017/08/15", "-i" };
            //args = new string[] { "-import", "predicta", "2017/08/10", "2017/08/20", "-p" };

            if (args[0].ToLower() == Constants.Import && args[1].ToLower() == Constants.EOD)
            {
                string[] eodImportArgs = new string[args.Length - 2];
                for (int i = 2, i1 = 0; i < args.Length; )
                {
                    eodImportArgs[i1++] = args[i++];
                }

                if (eodImportArgs.Contains(Constants.Securities))
                    EODSecuritiesImport.Execute(config, eodExchanges);
                if (eodImportArgs.Contains(Constants.Prices))
                    EODPricesImport.Execute(eodImportArgs, config, eodExchanges, 0);
                if (eodImportArgs.Contains(Constants.Indeces))
                    EODPricesImport.Execute(eodImportArgs, config, new string[] {Constants.EODIndecesImportFilenamePrefix}, Constants.IndecesExchangeCode);
            }

            else if (args[0].ToLower() == Constants.Import && args[1].ToLower() == Constants.Predicta)
            {
                string[] predictaImportArgs = new string[args.Length - 2];
                for (int i = 2, i1 = 0; i < args.Length; )
                {
                    predictaImportArgs[i1++] = args[i++];
                }

                PredictaImport.Execute(predictaImportArgs, config);
            }

            else if (args[0].ToLower() == Constants.Import && args[1].ToLower() == Constants.Quandl)
            {
                // string array of exchanges (Symbols) to download from Quandl
                string[] qndlExchanges = null;
                try
                {
                    qndlExchanges = config[Constants.QuandlExchanges].Split(new char[] { ',' });
                }
                catch
                {
                }

                // int array of exchanges id (DB table)
                int[] exchangeID = new int[qndlExchanges.Length];
                using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString_Quandl]))
                {
                    conn.Open();
                    for (int i = 0; i < qndlExchanges.Length; i++)
                    {
                        SqlCommand command = new SqlCommand("select idStockExchange from tblSel_StockExchanges where strSymbol = '" + qndlExchanges[i] + "'", conn);
                        exchangeID[i] = Convert.ToInt32(command.ExecuteScalar());
                    }
                }

                string[] quandlImportArgs = new string[args.Length - 2];
                for (int i = 2, i1 = 0; i < args.Length;)
                {
                    quandlImportArgs[i1++] = args[i++];
                }

                if (quandlImportArgs.Contains(Constants.Securities))
                    QuandlSecuritiesImport.Execute(config);

                //if (quandlImportArgs.Contains(Constants.Prices))
                //    QuandlPricesImport.Execute(config, exchangeID);

                //if (quandlImportArgs.Contains(Constants.HistPrices))
                //   QuandlHistPricesImport.Execute(config, exchangeID);
                //      OR  -- temporary hardcoded call

                if (quandlImportArgs.Contains(Constants.HistPrices) || quandlImportArgs.Contains(Constants.Prices))
                {
                    string wichPrices;
                    if (quandlImportArgs.Contains(Constants.Prices))
                        wichPrices = Constants.Prices;
                    else
                        wichPrices = Constants.HistPrices;

                    QuoteMediaPricesImport.Execute(config, exchangeID, wichPrices);
                }

                

                //if (eodImportArgs.Contains(Constants.Prices))
                //    EODPricesImport.Execute(eodImportArgs, config, eodExchanges, 0);
                //if (eodImportArgs.Contains(Constants.Indeces))
                //    EODPricesImport.Execute(eodImportArgs, config, new string[] { Constants.EODIndecesImportFilenamePrefix }, Constants.IndecesExchangeCode);
            }

            else if (args[0].ToLower() == Constants.Import && args[1].ToLower() == Constants.Intrinio)
            {
                // string array of exchanges (Symbols) to download from 'stock_exchanges.csv'

                //string[] intriExchanges = null;
                //try
                //{
                //    intriExchanges = config[Constants.IntrinioExchanges].Split(new char[] { ',' });
                //}
                //catch
                //{
                //}

                //         OR

                string[] intriExchanges = config[Constants.IntrinioExchanges].Split(new char[] { ',' });

                // int array of exchanges id (DB table)
                int[] exchangeID = new int[intriExchanges.Length];
                using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString_Quandl]))
                {
                    conn.Open();
                    for (int i = 0; i < intriExchanges.Length; i++)
                    {
                        SqlCommand command = new SqlCommand("select idStockExchange from tblSel_StockExchanges where strSymbol = '" + intriExchanges[i] + "'", conn);
                        exchangeID[i] = Convert.ToInt32(command.ExecuteScalar());
                    }
                }

                string[] intriImportArgs = new string[args.Length - 2];
                for (int i = 2, i1 = 0; i < args.Length;)
                {
                    intriImportArgs[i1++] = args[i++];
                }

                // ************** Securities import: initial and fundamental ******************
                if (intriImportArgs.Contains(Constants.Securities))
                {


                    ////IntrinioSecuritiesImport.Execute(config, exchangeID);
                    ////return;


                    // Initial import
                    if (intriImportArgs[0].ToLower() == Constants.Initial)
                    {
                        // ***************************************************
                        // USCOMP - US composite - securities import
                        IntrinioSecuritiesInitialImport.Execute(config);        // WARNING!!!  SKIP USCOMP securities import if testing TASE !!!!!!!!!!!!!!

                        // Historical prices import for NEW securities
                        List<string> secWithDividOrSplit = new List<string>();
                        IntrinioHistPricesImport.Execute(config, exchangeID, secWithDividOrSplit, false, true);  

                        // ***************************************************
                        // TASE securities import
                        IntrinioSecuritiesInitialImport.Execute(config, true);

                        // Historical prices import for NEW securities
                        int[] exchangeID_tase = new int[1] { 1 };
                        secWithDividOrSplit.Clear();
                        IntrinioHistPricesImport.Execute(config, exchangeID_tase, secWithDividOrSplit, true, true);
                    }




                    //************ FUNDAMENTALS *******************************
                    ////!! Uncomment 2 lines when fundamentals are ok !!   else // Fundamental import: only USCOMP
                    //// !!!!!!!!!    IntrinioSecuritiesImport.Execute(config, exchangeID); // DO NOT RUN BEFORE CHECKING WE HAVE DATA IN calls
                    //************************************************************

                }


                // **************** TEST QUANDL *******************************
                //if (quandlImportArgs.Contains(Constants.Prices))
                //    QuandlPricesImport.Execute(config, exchangeID);

                //if (quandlImportArgs.Contains(Constants.HistPrices))
                //   QuandlHistPricesImport.Execute(config, exchangeID);
                // ************************************************************


                //**************** ORIGINAL -- UNCOMMENT AFTER WEEKEND RUN  *************
                // ************** Prices import: Historical 
                if (intriImportArgs.Contains(Constants.HistPrices))             // Historical prices import: USA/TASE/USA Indices
                {
                    List<string> secWithDividOrSplit = new List<string>();
                    // USA exchanges
                    IntrinioHistPricesImport.Execute(config, exchangeID, secWithDividOrSplit);  // WARNING!!!  SKIP USA exchanges historical import if testing 'TASE exchange' or 'USA indices prices' !!!!!

                    //*******************************************************************************
                    // TASE exchange
                    int[] exchangeID_tase = new int[1] { 1 };
                    secWithDividOrSplit.Clear();
                    IntrinioHistPricesImport.Execute(config, exchangeID_tase, secWithDividOrSplit, true);

                    //*******************************************************************************
                    // USA indices prices - HISTORICAL
                    IntrinioHistIndicesPricesImport.Execute(config);
                }
                //*******************************************************************************
                ////////// CUSTOM HISTORY - WEEKEND RUN - USA HISTORICAL !!!!!!!!!!!!!!!!!
                ////////if (intriImportArgs.Contains(Constants.HistPrices))             // Historical prices import: USA/TASE/USA Indices
                ////////{
                ////////    List<string> secWithDividOrSplit = new List<string>();
                ////////    // USA exchanges
                ////////    IntrinioHistPricesImport.Execute(config, exchangeID, secWithDividOrSplit);  // WARNING!!!  SKIP USA exchanges historical import if testing 'TASE exchange' or 'USA indices prices' !!!!!

                ////////    //*******************************************************************************
                ////////    //// TASE exchange
                ////////    //int[] exchangeID_tase = new int[1] { 1 };
                ////////    //secWithDividOrSplit.Clear();
                ////////    //IntrinioHistPricesImport.Execute(config, exchangeID_tase, secWithDividOrSplit, true);

                ////////    //*******************************************************************************
                ////////    //// USA indices prices - HISTORICAL
                ////////    //IntrinioHistIndicesPricesImport.Execute(config);
                ////////}
                else if (intriImportArgs.Contains(Constants.Prices))            // Daily prices import: USA/TASE/USA Indices
                {
                    // USA exchanges
                    List<string> secWithDividOrSplit = new List<string>();
                    IntrinioPricesImport.Execute(intriImportArgs, config, exchangeID, secWithDividOrSplit);     // USCOMP prices import // WARNING!!!  SKIP USCOMP securities import if testing TASE !!!!!!!!!!!!!!
                    if (secWithDividOrSplit.Count > 0)
                    {
                        // Import historical prices for securities with divident or/and split
                        IntrinioHistPricesImport.Execute(config, exchangeID, secWithDividOrSplit);
                    }

                    // TASE exchange
                    int[] exchangeID_tase = new int[1] { 1 };
                    secWithDividOrSplit.Clear();
                    IntrinioPricesImport.Execute(intriImportArgs, config, exchangeID_tase, secWithDividOrSplit, true);     // TASE prices import
                    if (secWithDividOrSplit.Count > 0)
                    {
                        // Import historical prices for securities with divident or/and split
                        IntrinioHistPricesImport.Execute(config, exchangeID_tase, secWithDividOrSplit, true);
                    }

                    // USA indices prices - DAILY
                    IntrinioHistIndicesPricesImport.Execute(config, true);

                    // TASE USD/ILS exchange rate 9001 import
                    IntrinioPredictaImport.Execute(intriImportArgs, config);

                }
            }
            //////else if (args[0].ToLower() == Constants.CalcIntrinio)
            //////{
            //////    string[] intriExchanges = (config[Constants.IntrinioExchanges]).Split(new char[] { ',' });
            //////    // int array of exchanges id (DB table)
            //////    int[] exchangeID = new int[intriExchanges.Length];
            //////    using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString_Quandl]))
            //////    {
            //////        conn.Open();
            //////        for (int i = 0; i < intriExchanges.Length; i++)
            //////        {
            //////            SqlCommand command = new SqlCommand("select idStockExchange from tblSel_StockExchanges where strSymbol = '" + intriExchanges[i] + "'", conn);
            //////            exchangeID[i] = Convert.ToInt32(command.ExecuteScalar());
            //////        }
            //////    }

            //////    BatchCalculationsIntrinio_CUSTOM.Execute(config, exchangeID);
            //////}
            //**************** ORIGINAL -- UNCOMMENT AFTER WEEKEND RUN  *************
            else if (args[0].ToLower() == Constants.CalcIntrinio)
            {

                //// USA ONLY!!!!!!!!!  string[] intriExchanges = (config[Constants.IntrinioExchanges]).Split(new char[] { ',' });

                //// TASE ONLY!!!!!!!!!string[] intriExchanges = ("TASE").Split(new char[] { ',' });

                string[] intriExchanges = (config[Constants.IntrinioExchanges] + ",TASE").Split(new char[] { ',' });
                // int array of exchanges id (DB table)
                int[] exchangeID = new int[intriExchanges.Length];
                using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString_Quandl]))
                {
                    conn.Open();
                    for (int i = 0; i < intriExchanges.Length; i++)
                    {
                        SqlCommand command = new SqlCommand("select idStockExchange from tblSel_StockExchanges where strSymbol = '" + intriExchanges[i] + "'", conn);
                        exchangeID[i] = Convert.ToInt32(command.ExecuteScalar());
                    }
                }

                BatchCalculationsIntrinio.Execute(config, exchangeID);
            }
            //***********************************************************************
            else if (args[0].ToLower() == Constants.Calc)
            {
                int[] idExchanges = new int[exchanges.Length];
                using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString]))
                {
                    conn.Open();
                    for (int i = 0; i < exchanges.Length; i++)
                    {
                        SqlCommand command = new SqlCommand("select idStockExchange from tblSel_StockExchanges where strSymbol = '" + exchanges[i] + "'", conn);
                        idExchanges[i] = Convert.ToInt32(command.ExecuteScalar());
                    }
                }

                BatchCalculations.Execute(config, idExchanges);
            }


            else if (args[0].ToLower() == Constants.Scheduler)
            {
                for (; ; )
                {
                    int scheduleIntervalSecs = Convert.ToInt32(config[Constants.SchedulerSecondsToSleep]);
                    
                    bool runPredictaImport = false;
                    bool runEodIndecesImport = false;

                    List<string> lsExchanges = new List<string>();
                    List<int> idExchanges = new List<int>();

                    using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString]))
                    {

                        conn.Open();
                        SqlCommand command = new SqlCommand(
                            "select t.idStockExchange, e.strSymbol, t.scheduleTime from tbl_ExchangesTiming t left outer join tblSel_StockExchanges e on t.idStockExchange = e.idStockExchange",
                            conn);
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            DateTime scheduleTime1 = Convert.ToDateTime(reader[2]);
                            DateTime scheduleTime2 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, scheduleTime1.Hour, scheduleTime1.Minute, scheduleTime1.Second);
                            if (DateTime.Now.AddSeconds(-scheduleIntervalSecs) < scheduleTime2 && DateTime.Now.AddSeconds(60) > scheduleTime2)
                            {
                                int exchangeId = Convert.ToInt32(reader[0]);
                                if (exchangeId == Constants.PredictaExchangeId)
                                {
                                    runPredictaImport = true;
                                }
                                else if (exchangeId == Constants.IndecesExchangeCode)
                                {
                                    runEodIndecesImport = true;
                                }
                                else
                                {
                                    idExchanges.Add(exchangeId);
                                    lsExchanges.Add(Convert.ToString(reader[1]));
                                }
                            }
                        }
                    }

                    if (runEodIndecesImport || runPredictaImport || idExchanges.Count > 0)
                    { // Main scheduler
                        ThreadStart starter = delegate { ScheduleCherriesBatchProcessTask(runPredictaImport, runEodIndecesImport, idExchanges.ToArray(), lsExchanges.ToArray(), config); };
                        Thread thread = new Thread(starter);
                        thread.Start();
                    }
                    
                    Thread.Sleep(scheduleIntervalSecs * 1000);
                }
            }
        }

        private static void ScheduleCherriesBatchProcessTask(bool runPredictaImport, bool runEodIndecesImport, int[] idExchanges, string[] exchanges, Dictionary<string, string> config)
        { // Thread process
            if (runPredictaImport)
            {
                PredictaImport.Execute(new string[] { Constants.Last, Constants.Securities, Constants.Prices, Constants.Indeces }, config);
                BatchCalculations.Execute(config, new int[] { Constants.PredictaExchangeId });
            }

            if (runEodIndecesImport)
            {
                EODPricesImport.Execute(new string[] { Constants.Last, Constants.Indeces }, config, new string[] { Constants.EODIndecesImportFilenamePrefix }, Constants.IndecesExchangeCode);
                BatchCalculations.Execute(config, new int[] { Constants.IndecesExchangeCode });
            }

            if (idExchanges.Length > 0)
            {
                EODSecuritiesImport.Execute(config, exchanges);
                EODPricesImport.Execute(new string[] { Constants.Last, Constants.Prices }, config, exchanges, 0);
                BatchCalculations.Execute(config, idExchanges);
            }
        }
    }
}
