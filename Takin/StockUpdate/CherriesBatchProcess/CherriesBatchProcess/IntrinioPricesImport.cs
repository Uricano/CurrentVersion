using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace CherriesBatchProcess
{
    public class IntrinioPricesImport
    {
        public static void Execute(string[] args, Dictionary<string, string> config, int[] exchangeID, List<string> secWithDividOrSplit, bool isTASE = false)
        {
            string clientFolder = null;
            string processedFolder = null;
            string rejectedFolder = null;
            string ftpServerUrlstart = null;
            //string ftpServerUrl = null;
            string ftpUserName = null;
            string ftpPassword = null;
            string doDeleteFiles = null;
            int number_of_pages = 2;
            double shekToAgorot;

            string intriMediaURL;
            DateTime importDate = DateTime.Today.AddDays(-1);   // For 'Daily...' import
            DateTime startDate = importDate;
            DateTime endDate = importDate;

            clientFolder = config[Constants.SourceDir];
            processedFolder = config[Constants.ProcessedDir];
            rejectedFolder = config[Constants.RejectedDir];
            ftpServerUrlstart = config[Constants.IntrinioPricesFTPServerURL];
            //ftpServerUrl = string.Format("{0}{1}&{2}", ftpServerUrlstart, "AAPL", config[Constants.QuandlAPIkey]);
            ftpUserName = config[Constants.IntrinioSecuritiesFTPUserName];
            ftpPassword = config[Constants.IntrinioSecuritiesFTPPassword];
            string fullUrl;
            doDeleteFiles = config[Constants.DoDeleteFiles];

            if (isTASE)
            {
                // TASE daily prices
                intriMediaURL = @"https://api.intrinio.com/prices/exchange.csv?identifier=^XTAE&price_date={0}&page_number={1}";
                number_of_pages = 1;
                shekToAgorot = 100.0;
            }
            else 
            {
                // Daily(yesterday) or for date periods
                intriMediaURL = @"https://api.intrinio.com/prices/exchange.csv?identifier=^USCOMP&price_date={0}&page_number={1}";
                number_of_pages = 2;
                shekToAgorot = 1.0;
            }

            using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString_Quandl].ToString()))
            {
                conn.Open();
                Logger logger = new Logger(conn, Constants.IntrinioPricesImport);
                string sp = null;

                string filename = null;
                string fileContent = null;
                ////////string intri_symbol;
                ////////string id_exchange;
                List<string> listSymbols = new List<string>();
                DataTable importedSecuritiesTable = getPricesTableStructure();

                // Define if Daily or Date range
                if (args[0].ToLower() != Constants.Daily)
                {
                    startDate = DateTime.ParseExact(args[0], Constants.DateParmsFormat, System.Globalization.CultureInfo.CurrentCulture);
                    endDate = DateTime.ParseExact(args[1], Constants.DateParmsFormat, System.Globalization.CultureInfo.CurrentCulture);
                }

                List<DateTime> listImportDates = GetDateRange(startDate, endDate);

                //////// Get securities
                //////string query = string.Format("SELECT  idStockExchange, intrinioSymbol FROM tblSel_StockExchanges where idStockExchange in ({0})", string.Join(", ", exchangeID));
                //////DataTable dtIntriSymbol = getDBTable(query, conn);

                //////for (int i = 0; i < dtIntriSymbol.Rows.Count; i++)
                //////{
                //////    id_exchange = dtIntriSymbol.Rows[i]["idStockExchange"].ToString();
                //////    intri_symbol = dtIntriSymbol.Rows[i]["intrinioSymbol"].ToString();

                //      OR

                // In case of date range we have list of dates
                for (int i = 0; i < listImportDates.Count; i++)
                {
                    importDate = listImportDates[i];

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    listSymbols.Clear();    // In case of date range - have to clear the list for each date
                    importedSecuritiesTable.Rows.Clear();

                    for (int ll = 1; ll <= number_of_pages; ll++)
                    {
                        try
                        {
                            // Daily and for date periods
                            fullUrl = string.Format(intriMediaURL, importDate.ToString("yyyy-MM-dd"), ll);

                            filename = string.Format("INTRINIO_Prices_page_{0}_{1}.txt", ll, ((isTASE) ? "TASE" : "USA"));
                            string fullFilename = clientFolder + "\\" + filename;
                            HttpWebRequest requestf = (HttpWebRequest)WebRequest.Create(fullUrl);
                            requestf.KeepAlive = false;
                            requestf.Method = WebRequestMethods.File.DownloadFile;
                            requestf.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                            HttpWebResponse respf = (HttpWebResponse)requestf.GetResponse();
                            Stream respfStream = respf.GetResponseStream();
                            StreamReader readerf = new StreamReader(respfStream);
                            fileContent = readerf.ReadToEnd();
                            readerf.Close();
                            respf.Close();
                            if (!Directory.Exists(clientFolder)) Directory.CreateDirectory(clientFolder);
                            File.WriteAllText(fullFilename, fileContent);

                            //logger.WriteLog(Constants.Success, filename, DateTime.MinValue, null, "Succesfull Intrinio prices FTP file import - " + sec_symbol, 1, null);
                        }
                        catch (Exception ex)
                        {
                            logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, null, string.Format("Unsuccesfull Intrinio {0} prices FTP file import for {1}", ((isTASE) ? "TASE" : "USA"), importDate.ToString("dd-MM-yyyy")), 0, ex);
                            return; // skip to next security
                        }

                        try
                        {

                            string[] securityRecords = fileContent.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                            int j = 0;
                            // loop by imported securities
                            foreach (string securityRec in securityRecords)
                            {
                                j++;
                                if (j == 1 || j == 2) continue;   // skip lines with field titles

                                DataRow importedSecuruty = importedSecuritiesTable.NewRow();
                                if(getPriceRow(importedSecuruty, securityRec, shekToAgorot))
                                {
                                    if (!listSymbols.Contains(importedSecuruty["symbol"].ToString().ToUpper()))
                                    {
                                        // Add security for price import
                                        importedSecuritiesTable.Rows.Add(importedSecuruty);
                                        importedSecuritiesTable.AcceptChanges();

                                        // Build a list of symbols not to have duplicate entries for the same date, which causes problems in 'Insert...' in SP
                                        listSymbols.Add(importedSecuruty["symbol"].ToString().ToUpper());

                                        // Build a list of securities with dividents or/and splits
                                        if (importDate == DateTime.Today.AddDays(-1))
                                        {
                                            if (Convert.ToDouble(importedSecuruty["dividend"]) != 0)
                                                secWithDividOrSplit.Add(importedSecuruty["symbol"].ToString().ToUpper());
                                            else if (Convert.ToDouble(importedSecuruty["split"]) != 1)
                                                secWithDividOrSplit.Add(importedSecuruty["symbol"].ToString().ToUpper());
                                        }
                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, sp, "Unsuccesfull Intrinio prices table creation", 0, ex);
                        }

                        if (importedSecuritiesTable.Rows.Count < 1)
                            continue; // no need to process 2nd page if the first was empty - exit 'For ll ...' loop

                    }// For ll... for 2 pages of prices

                    if (importedSecuritiesTable.Rows.Count < 1)
                        continue; // go to the next date in case of date range - exit 'For i ...' loop

                    try
                    {
                        sp = Constants.importIntrinioPrices;    // "importIntrinioPrices";    
                        SqlCommand securityImportCommand = new SqlCommand(String.Empty, conn);
                        securityImportCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        securityImportCommand.CommandText = sp;
                        securityImportCommand.CommandTimeout = Constants.SPsTimeout;
                        securityImportCommand.Parameters.Add("@imported_prices", SqlDbType.Structured).Value = importedSecuritiesTable;
                        securityImportCommand.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = string.Join(", ", exchangeID);    // id_exchange; //TODO: CHANGE LATER when know what to do with symbols in 2 exchanges
                        securityImportCommand.Parameters.Add("@is_historical", SqlDbType.Int).Value = 0;
                        //securityImportCommand.Parameters.Add("@symbol", SqlDbType.VarChar).Value = sec_symbol;  // importedSecuritiesTable.Rows[0]["symbol"];
                        //securityImportCommand.Parameters.Add("@id_security", SqlDbType.VarChar).Value = id_Security;    // importedSecuritiesTable.Rows[0]["symbol"];
                        //securityImportCommand.Parameters.Add("@start_date", SqlDbType.DateTime).Value = Convert.ToDateTime(importedSecuritiesTable.Rows[0]["date"]);
                        //securityImportCommand.Parameters.Add("@end_date", SqlDbType.DateTime).Value = Convert.ToDateTime(importedSecuritiesTable.Rows[importedSecuritiesTable.Rows.Count - 1]["date"]);
                        securityImportCommand.ExecuteNonQuery();

                        string sourceFileFullName = clientFolder + "\\" + filename;
                        string processedFileFullName = processedFolder + "\\" + filename;

                        if (doDeleteFiles == "0")
                            File.WriteAllText(processedFileFullName, fileContent);

                        File.Delete(sourceFileFullName);

                        stopwatch.Stop();
                        //elapsed_time = stopwatch.ElapsedMilliseconds;
                        
                        logger.WriteLog(Constants.Success, filename, DateTime.MinValue, sp, string.Format("Succesfull Intrinio {0} prices import for {1}", ((isTASE) ? "TASE" : "USA"), importDate.ToString("dd-MM-yyyy")), 1, null);
                        //logger.WriteLog(Constants.Success, filename, DateTime.MinValue, sp, string.Format("Succesfull Intrinio prices import - {0} ms for {1}", stopwatch.ElapsedMilliseconds.ToString(), importDate.ToString("dd-MM-yyyy")), 1, null);

                        if (args[0].ToLower() == Constants.Daily)
                        {
                            for (int xx = 0; xx < exchangeID.Length; xx++)
                            {
                                GeneralFunctions.SetLastUpdateDate(conn, exchangeID[xx]);     
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, sp, string.Format("Unsuccesfull Intrinio {0} prices import for {1}", ((isTASE) ? "TASE" : "USA"), importDate.ToString("dd-MM-yyyy")), 0, ex);
                    }

                }// For i... for date range

                //logger.WriteLog(Constants.Success, filename, DateTime.MinValue, sp, string.Format("Succesfull Intrinio {0} prices import - {1} ms", ((whichPrices == Constants.Prices) ? "daily" : "historical"), stopwatch.ElapsedMilliseconds.ToString()), 1, null);

            }// using
        }

        private static bool getPriceRow(DataRow importedSecuruty, string securityRec, double shekToAgorot)
        {
            try
            {
                string[] security = securityRec.Split(new char[] { ',' });
                string symb = security[0].Trim();
                if (symb.IndexOf("-") > 0 || symb.IndexOf(".") > 0)
                    return false;

                importedSecuruty["symbol"] = symb;
                importedSecuruty["date"] = security[3];
                importedSecuruty["open"] = Convert.ToDouble(security[4]) * shekToAgorot;
                importedSecuruty["close"] = Convert.ToDouble(security[7]) * shekToAgorot;
                importedSecuruty["volume"] = security[8].Trim();    // (security[8].Trim() == string.Empty || security[8].Trim() == string.Empty) ? DBNull.Value : security[8].Trim());
                importedSecuruty["dividend"] = security[9];
                importedSecuruty["split"] = security[10];
                importedSecuruty["adjOpen"] = Convert.ToDouble(security[11]) * shekToAgorot;
                importedSecuruty["adjClose"] = Convert.ToDouble(security[14]) * shekToAgorot;
                importedSecuruty["adjVolume"] = security[15];
                //return importedSecuruty;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static DataTable getPricesTableStructure()
        {
            DataTable importedSecuritiesTable = new DataTable();
            try
            {
                importedSecuritiesTable.Columns.Add("symbol");
                importedSecuritiesTable.Columns.Add("date");
                importedSecuritiesTable.Columns.Add("open");
                importedSecuritiesTable.Columns.Add("close");
                importedSecuritiesTable.Columns.Add("volume");
                importedSecuritiesTable.Columns.Add("dividend");
                importedSecuritiesTable.Columns.Add("split");
                importedSecuritiesTable.Columns.Add("adjOpen");
                importedSecuritiesTable.Columns.Add("adjClose");
                importedSecuritiesTable.Columns.Add("adjVolume");
            }
            catch (Exception ex)
            {
            }
            return importedSecuritiesTable;
        }

        // Fill in table all securities IDs
        public static DataTable getDBTable(string query, SqlConnection conn)
        {
            DataTable dataTable = new DataTable();
            //string connString = @"your connection string here";
            //string query = string.Format("select idSecurity, strSymbol from tbl_Securities where idSecurityType = 1 and idStockExchange in ({0}), )";

            //SqlConnection conn = new SqlConnection(connString);
            try
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                //conn.Open();

                // create data adapter
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                //conn.Close();
                da.Dispose();
            }
            catch (Exception ex)
            {
            }

            return dataTable;
        }

        private static List<DateTime> GetDateRange(DateTime StartingDate, DateTime EndingDate)
        {
            if (StartingDate > EndingDate)
            {
                return null;
            }
            List<DateTime> rv = new List<DateTime>();
            DateTime tmpDate = StartingDate;
            do
            {
                rv.Add(tmpDate);
                tmpDate = tmpDate.AddDays(1);
            } while (tmpDate <= EndingDate);
            return rv;
        }
    }
}
