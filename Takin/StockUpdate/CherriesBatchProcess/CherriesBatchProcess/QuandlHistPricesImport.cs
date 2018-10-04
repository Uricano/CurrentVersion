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

namespace CherriesBatchProcess
{
    public class QuandlHistPricesImport
    {
        public static void Execute(Dictionary<string, string> config, int[] exchangeID)
        {
            string clientFolder = null;
            string processedFolder = null;
            string rejectedFolder = null;
            string ftpServerUrlstart = null;
            //string ftpServerUrl = null;
            string ftpUserName = null;
            string ftpPassword = null;
            string doDeleteFiles = null;

            clientFolder = config[Constants.SourceDir];
            processedFolder = config[Constants.ProcessedDir];
            rejectedFolder = config[Constants.RejectedDir];
            ftpServerUrlstart = config[Constants.QuandlHistPricesFTPServerURL];
            //ftpServerUrl = string.Format("{0}{1}&{2}", ftpServerUrlstart, "AAPL", config[Constants.QuandlAPIkey]);
            ftpUserName = config[Constants.QuandlPricesFTPUserName];
            ftpPassword = config[Constants.QuandlPricesFTPPassword];
            doDeleteFiles = config[Constants.DoDeleteFiles];

            using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString_Quandl].ToString()))
            {
                conn.Open();
                Logger logger = new Logger(conn, Constants.QuandlPricesImport);

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

                    try
                    {
                        filename = string.Format("QUANDL_HistPrices_{0}.txt", sec_symbol);
                        string fullUrl = string.Format("{0}{1}&{2}", ftpServerUrlstart, sec_symbol, config[Constants.QuandlAPIkey]); ;
                        //string fullUrl =  httpfilename;
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

                        logger.WriteLog(Constants.Success, filename, DateTime.MinValue, null, "Succesfull Quandl historical prices FTP file import - " + sec_symbol, 1, null);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, null, "Unsuccesfull Quandl historical prices FTP file import - " + sec_symbol, 0, ex);
                        continue; // skip to next security
                    }

                    string sp = null;
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

                        sp = Constants.importQuandlHistPrices;
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

                        //sp = null;
                        string sourceFileFullName = clientFolder + "\\" + filename;
                        string processedFileFullName = processedFolder + "\\" + filename;

                        if (doDeleteFiles == "0")
                            File.WriteAllText(processedFileFullName, fileContent);

                        File.Delete(sourceFileFullName);

                        //sp = Constants.importQuandlPrices;
                        logger.WriteLog(Constants.Success, filename, DateTime.MinValue, sp, "Succesfull Quandl prices import - " + sec_symbol, 1, null);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, sp, "Unsuccesfull Quandl prices import - " + sec_symbol, 0, ex);
                    }
                }// for
            }// using
        }

        private static void getPriceRow(DataRow importedSecuruty, string securityRec)
        {
            try
            {
                string[] security = securityRec.Split(new char[] { ',' });

                importedSecuruty["symbol"] = security[0];
                importedSecuruty["date"] = security[1];
                importedSecuruty["open"] = security[2];
                importedSecuruty["close"] = security[5];
                importedSecuruty["volume"] = security[6];
                importedSecuruty["dividends"] = security[7];

                //importedSecuruty["split"] = security[8];
                //importedSecuruty["adjOpen"] = security[9];
                //importedSecuruty["adjClose"] = security[12];
                //importedSecuruty["adjVolume"] = security[13];
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
                importedSecuritiesTable.Columns.Add("symbol");
                importedSecuritiesTable.Columns.Add("date");
                importedSecuritiesTable.Columns.Add("open");
                importedSecuritiesTable.Columns.Add("close");
                importedSecuritiesTable.Columns.Add("volume");
                importedSecuritiesTable.Columns.Add("dividends");

                //importedSecuritiesTable.Columns.Add("split");
                //importedSecuritiesTable.Columns.Add("adjOpen");
                //importedSecuritiesTable.Columns.Add("adjClose");
                //importedSecuritiesTable.Columns.Add("adjVolume");
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
