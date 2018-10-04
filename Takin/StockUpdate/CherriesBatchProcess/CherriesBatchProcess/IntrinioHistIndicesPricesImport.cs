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
    public class IntrinioHistIndicesPricesImport
    {
        public static void Execute(Dictionary<string, string> config, bool isDaily = false)   //, int[] exchangeID, bool isTASE = false)
        {
            string clientFolder = null;
            string processedFolder = null;
            string rejectedFolder = null;
            string ftpServerUrlstart = null;
            //string ftpServerUrl = null;
            string ftpUserName = null;
            string ftpPassword = null;
            string doDeleteFiles = null;
            double shekToAgorot;
            string intriMediaURL;
            DateTime minImportDate = Convert.ToDateTime("2012-01-01");  // First time historical prices import only
            if (isDaily)
                minImportDate = DateTime.Today.AddDays(-6);             // Daily prices import //for testing - Convert.ToDateTime("2018-04-26"); // 

            clientFolder = config[Constants.SourceDir];
            processedFolder = config[Constants.ProcessedDir];
            rejectedFolder = config[Constants.RejectedDir];
            ftpServerUrlstart = config[Constants.IntrinioPricesFTPServerURL];
            //ftpServerUrl = string.Format("{0}{1}&{2}", ftpServerUrlstart, "AAPL", config[Constants.QuandlAPIkey]);
            ftpUserName = config[Constants.IntrinioSecuritiesFTPUserName];
            ftpPassword = config[Constants.IntrinioSecuritiesFTPPassword];
            string fullUrl;
            doDeleteFiles = config[Constants.DoDeleteFiles];

            //// Historical prices
            //if (isTASE)
            //{
            //    intriMediaURL = @"https://api.intrinio.com/prices.csv?ticker={0}";
            //    shekToAgorot = 100.0;
            //}
            //else
            //{
                intriMediaURL = @"https://api.intrinio.com/prices.csv?ticker={0}";
                shekToAgorot = 1.0;
            //}

            using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString_Quandl].ToString()))
            {
                conn.Open();
                Logger logger = new Logger(conn, Constants.IntrinioPricesImport);
                //string sp = null;

                string filename = null;
                string fileContent = null;
                string sec_symbol;

                // Table of data
                DataTable importedSecuritiesTable = getPricesTableStructure();

                // Get securities   $COMPQ
                string query = "SELECT  intrinioSymbol FROM tbl_Indices where intrinioSymbol is not null order by intrinioSymbol";
                //string query = "SELECT  intrinioSymbol FROM tbl_Indices where intrinioSymbol = '$COMPQ'";

                DataTable dtSecurities = getDBTable(query, conn);

                for (int s = 0; s < dtSecurities.Rows.Count; s++)
                {
                    try
                    {
                        // Historical prices
                        sec_symbol = dtSecurities.Rows[s]["intrinioSymbol"].ToString();
                        fullUrl = string.Format(intriMediaURL, sec_symbol);

                        filename = "INTRINIO_Hist_Indices_Prices.txt";
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
                        logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, null, "Unsuccesfull Intrinio historical prices FTP file import ", 0, ex);
                        continue; // skip to next security
                    }

                    // Clear table for each security
                    if (!isDaily)
                        importedSecuritiesTable.Rows.Clear();

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
                            if (getPriceRow(importedSecuruty, securityRec, sec_symbol, shekToAgorot))
                            {
                                if (Convert.ToDateTime(importedSecuruty["date"].ToString()) >= minImportDate)   //== minImportDate)   // for 1 day only
                                {
                                    importedSecuritiesTable.Rows.Add(importedSecuruty);
                                    importedSecuritiesTable.AcceptChanges();
                                }
                            }
                        }

                        if (importedSecuritiesTable.Rows.Count < 1)
                            continue;
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, null, "Unsuccesfull Intrinio prices table creation", 0, ex);
                        continue;
                    }

                    // Historical import for each security
                    if (!isDaily)
                    {
                        if (!importIntrinioIndicesPrices(logger, importedSecuritiesTable, doDeleteFiles,
                                                     clientFolder, processedFolder, rejectedFolder,
                                                     filename, fileContent, sec_symbol, isDaily, conn))
                            continue;

                    }
                }// For... for next security

                // Daily import
                if (isDaily && importedSecuritiesTable.Rows.Count > 0)
                {
                    importIntrinioIndicesPrices(logger, importedSecuritiesTable, doDeleteFiles,
                                                 clientFolder, processedFolder, rejectedFolder,
                                                 filename, fileContent, "", isDaily, conn);
                }

            }// using
        }


        private static bool importIntrinioIndicesPrices(Logger logger, DataTable importedSecuritiesTable, string doDeleteFiles, string clientFolder, string processedFolder, string rejectedFolder,
                                                        string filename, string fileContent, string sec_symbol, bool isDaily, SqlConnection conn)
        {
            string sp = Constants.importIntrinioHistIndicesPrices;
            try
            {
                SqlCommand securityImportCommand = new SqlCommand(String.Empty, conn);
                securityImportCommand.CommandType = System.Data.CommandType.StoredProcedure;
                securityImportCommand.CommandText = sp;
                securityImportCommand.CommandTimeout = Constants.SPsTimeout;
                securityImportCommand.Parameters.Add("@imported_prices", SqlDbType.Structured).Value = importedSecuritiesTable;
                securityImportCommand.ExecuteNonQuery();

                string sourceFileFullName = clientFolder + "\\" + filename;
                string processedFileFullName = processedFolder + "\\" + filename;

                if (doDeleteFiles == "0")
                    File.WriteAllText(processedFileFullName, fileContent);

                File.Delete(sourceFileFullName);

                logger.WriteLog(Constants.Success, filename, DateTime.MinValue, sp, string.Format("Succesfull Intrinio {0} indices prices import " + sec_symbol, (isDaily? "daily": "historical")), 1, null);
                return true;
            }
            catch (Exception ex)
            {
                logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, sp, string.Format("Unsuccesfull Intrinio {0} indices prices import " + sec_symbol, (isDaily ? "daily" : "historical")), 0, ex);
                return false;
            }

        }

        private static bool getPriceRow(DataRow importedSecuruty, string securityRec, string sec_symbol, double shekToAgorot)
        {
            try
            {
                string[] security = securityRec.Split(new char[] { ',' });

                importedSecuruty["intrinioSymbol"] = sec_symbol;
                importedSecuruty["date"] = security[0];
                importedSecuruty["close"] = Convert.ToDouble(security[4]) * shekToAgorot;
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
                importedSecuritiesTable.Columns.Add("intrinioSymbol");
                importedSecuritiesTable.Columns.Add("date");
                importedSecuritiesTable.Columns.Add("close");
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
