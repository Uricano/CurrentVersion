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
    public class QuoteMediaPricesImport
    {

        public static void Execute(Dictionary<string, string> config, int[] exchangeID, string whichPrices)
        {
            string clientFolder = null;
            string processedFolder = null;
            string rejectedFolder = null;
            string ftpServerUrlstart = null;
            //string ftpServerUrl = null;
            string ftpUserName = null;
            string ftpPassword = null;
            string doDeleteFiles = null;
            string quoteMediaURL;
            DateTime importDate = DateTime.Today.AddDays(-4);     // (-1);

            clientFolder = config[Constants.SourceDir];
            processedFolder = config[Constants.ProcessedDir];
            rejectedFolder = config[Constants.RejectedDir];
            ftpServerUrlstart = config[Constants.QuandlHistPricesFTPServerURL];
            //ftpServerUrl = string.Format("{0}{1}&{2}", ftpServerUrlstart, "AAPL", config[Constants.QuandlAPIkey]);
            ftpUserName = config[Constants.QuandlPricesFTPUserName];
            ftpPassword = config[Constants.QuandlPricesFTPPassword];
            string fullUrl;
            doDeleteFiles = config[Constants.DoDeleteFiles];

            if (whichPrices == Constants.Prices)
            {
                // Daily(yesterday) or for date periods
                quoteMediaURL = @"https://www.quandl.com/api/v3/datasets/EOD/{0}.csv?start_date={1}&end_date={2}&{3}";
            }
            else //if (whichPrices == Constants.HistPrices)
            {
                // Historical prices
                quoteMediaURL = @"https://www.quandl.com/api/v3/datasets/EOD/{0}.csv?order=asc&{1}";
            }

            using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString_Quandl].ToString()))
            {
                conn.Open();
                Logger logger = new Logger(conn, Constants.QuandlPricesImport);
                string sp = null;

                string filename = null;
                string fileContent = null;
                string sec_symbol;
                string id_Security;

                // Get securities
                string query = string.Format("select idSecurity, strSymbol from tbl_Securities where idSecurityType = 1 and idStockExchange in ({0})", string.Join(", ", exchangeID));
                DataTable dtSecurities = getDBTable(query, conn);

                for (int i = 0; i < dtSecurities.Rows.Count; i++)
                {
                    sec_symbol = dtSecurities.Rows[i]["strSymbol"].ToString();
                    id_Security = dtSecurities.Rows[i]["idSecurity"].ToString();

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    try
                    {
                        filename = string.Format("QUOTEMEDIA_Prices_{0}.txt", sec_symbol);

                        if (whichPrices == Constants.Prices)
                        {
                            // Daily and for date periods
                            fullUrl = string.Format(quoteMediaURL, sec_symbol, importDate.ToString("yyyy-MM-dd"), importDate.ToString("yyyy-MM-dd"), config[Constants.QuandlAPIkey]);
                        }
                        else //if (whichPrices == Constants.HistPrices)
                        {
                            // Historical prices
                            fullUrl = string.Format(quoteMediaURL, sec_symbol, config[Constants.QuandlAPIkey]);
                        }

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

                        //logger.WriteLog(Constants.Success, filename, DateTime.MinValue, null, "Succesfull QuoteMedia prices FTP file import - " + sec_symbol, 1, null);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, null, "Unsuccesfull QuoteMedia prices FTP file import - " + sec_symbol, 0, ex);
                        continue; // skip to next security
                    }

                    try
                    {
                        DataTable importedSecuritiesTable = getPricesTableStructure();

                        string[] securityRecords = fileContent.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        int j = 0;
                        // loop by imported securities
                        foreach (string securityRec in securityRecords)
                        {
                            j++;
                            if (j == 1) continue;   // skip line with field titles

                            DataRow importedSecuruty = importedSecuritiesTable.NewRow();
                            getPriceRow(importedSecuruty, securityRec);
                            importedSecuritiesTable.Rows.Add(importedSecuruty);
                            importedSecuritiesTable.AcceptChanges();
                        }

                        if (importedSecuritiesTable.Rows.Count < 1)
                            continue;

                        sp = "importQuoteMediaHistoricalPrices";    // Constants.importQuandlHistPrices;
                        SqlCommand securityImportCommand = new SqlCommand(String.Empty, conn);
                        securityImportCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        securityImportCommand.CommandText = sp;
                        securityImportCommand.CommandTimeout = Constants.SPsTimeout;
                        securityImportCommand.Parameters.Add("@imported_prices", SqlDbType.Structured).Value = importedSecuritiesTable;
                        securityImportCommand.Parameters.Add("@symbol", SqlDbType.VarChar).Value = sec_symbol;  // importedSecuritiesTable.Rows[0]["symbol"];
                        securityImportCommand.Parameters.Add("@id_security", SqlDbType.VarChar).Value = id_Security;    // importedSecuritiesTable.Rows[0]["symbol"];
                        securityImportCommand.Parameters.Add("@start_date", SqlDbType.DateTime).Value = Convert.ToDateTime(importedSecuritiesTable.Rows[0]["date"]);
                        securityImportCommand.Parameters.Add("@end_date", SqlDbType.DateTime).Value = Convert.ToDateTime(importedSecuritiesTable.Rows[importedSecuritiesTable.Rows.Count - 1]["date"]);
                        securityImportCommand.ExecuteNonQuery();

                        string sourceFileFullName = clientFolder + "\\" + filename;
                        string processedFileFullName = processedFolder + "\\" + filename;

                        if (doDeleteFiles == "0")
                            File.WriteAllText(processedFileFullName, fileContent);

                        File.Delete(sourceFileFullName);

                        stopwatch.Stop();
                        //elapsed_time = stopwatch.ElapsedMilliseconds;

                        logger.WriteLog(Constants.Success, filename, DateTime.MinValue, sp, string.Format("Succesfull QuoteMedia prices import - {0} - {1} ms", sec_symbol, stopwatch.ElapsedMilliseconds.ToString()), 1, null);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, sp, "Unsuccesfull QuoteMedia prices import - " + sec_symbol, 0, ex);
                    }
                }// for

                logger.WriteLog(Constants.Success, filename, DateTime.MinValue, sp, string.Format("Succesfull QuoteMedia {0} prices import", ((whichPrices == Constants.Prices) ? "daily": "historical")), 1, null);

            }// using
        }

        private static void getPriceRow(DataRow importedSecuruty, string securityRec)
        {
            try
            {
                string[] security = securityRec.Split(new char[] { ',' });

                importedSecuruty["date"] = security[0];
                importedSecuruty["open"] = security[1];
                importedSecuruty["close"] = security[4];
                importedSecuruty["volume"] = security[5];
                importedSecuruty["dividend"] = security[6];
                importedSecuruty["split"] = security[7];
                importedSecuruty["adjOpen"] = security[8];
                importedSecuruty["adjClose"] = security[11];
                importedSecuruty["adjVolume"] = security[12];
                //return importedSecuruty;
            }
            catch (Exception ex)
            {
            }
        }

        private static DataTable getPricesTableStructure()
        {
            DataTable importedSecuritiesTable = new DataTable();
            try
            {
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
    }
}
