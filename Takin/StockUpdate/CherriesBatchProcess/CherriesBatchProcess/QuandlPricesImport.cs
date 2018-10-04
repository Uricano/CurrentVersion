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
    public class QuandlPricesImport
    {

        public static void Execute(Dictionary<string, string> config, int[] exchangeID)
        {
            string clientFolder = null;
            string processedFolder = null;
            string rejectedFolder = null;
            string ftpServerUrlstart = null;
            string ftpServerUrl = null;
            string ftpUserName = null;
            string ftpPassword = null;
            string doDeleteFiles = null;
            DateTime importDate = DateTime.Today.AddDays(-1);

            clientFolder = config[Constants.SourceDir];
            processedFolder = config[Constants.ProcessedDir];
            rejectedFolder = config[Constants.RejectedDir];
            ftpServerUrlstart = config[Constants.QuandlPricesFTPServerURL];
            ftpServerUrl = string.Format("{0}{1}&{2}", ftpServerUrlstart, importDate.ToString("yyyy-MM-dd"), config[Constants.QuandlAPIkey]);
            ////ftpServerUrl = string.Format("{0}{1}&{2}", ftpServerUrlstart, "2018-03-05", config[Constants.QuandlAPIkey]);
            ftpUserName = config[Constants.QuandlPricesFTPUserName];
            ftpPassword = config[Constants.QuandlPricesFTPPassword];
            doDeleteFiles = config[Constants.DoDeleteFiles];
            
            using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString_Quandl].ToString()))
            {
                conn.Open();
                Logger logger = new Logger(conn, Constants.QuandlPricesImport);

                //string httpfilename = "https://www.quandl.com/api/v3/datatables/WIKI/PRICES.csv?date=2018-02-28&api_key=ULxnhY2CBervENYkyNri";
                string filename = null;
                string fileContent = null;

                try
                {
                    filename = "QUANDL_Prices.txt";
                    string fullUrl = ftpServerUrl;
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

                    logger.WriteLog(Constants.Success, filename, DateTime.MinValue, null, "Succesfull Quandl prices FTP file import", 1, null);
                }
                catch (Exception ex)
                {
                    logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, null, "Unsuccesfull Quandl prices FTP file import", 0, ex);
                    return;
                }

                string sp = null;
                try
                {
                    DataTable importedSecuritiesTable = getPricesTableStructure();

                    string[] securityRecords = fileContent.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    int i = 0;
                    // loop by imported securities
                    foreach (string securityRec in securityRecords)
                    {
                        i++;
                        if (i == 1) continue;

                        DataRow importedSecuruty = importedSecuritiesTable.NewRow();
                        getPriceRow(importedSecuruty, securityRec);
                        importedSecuritiesTable.Rows.Add(importedSecuruty);
                        importedSecuritiesTable.AcceptChanges();
                    }

                    sp = Constants.importQuandlPrices;
                    SqlCommand securityImportCommand = new SqlCommand(String.Empty, conn);
                    securityImportCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    securityImportCommand.CommandText = sp;
                    securityImportCommand.CommandTimeout = Constants.SPsTimeout;
                    securityImportCommand.Parameters.Add("@imported_prices", SqlDbType.Structured).Value = importedSecuritiesTable;
                    securityImportCommand.Parameters.Add("@date", SqlDbType.DateTime).Value = importDate;
                    securityImportCommand.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = string.Join(",", exchangeID);
                    securityImportCommand.ExecuteNonQuery();

                    //sp = null;
                    string sourceFileFullName = clientFolder + "\\" + filename;
                    string processedFileFullName = processedFolder + "\\" + filename;

                    if (doDeleteFiles == "0")
                        File.WriteAllText(processedFileFullName, fileContent);

                    File.Delete(sourceFileFullName);

                    //sp = Constants.importQuandlPrices;
                    logger.WriteLog(Constants.Success, filename, DateTime.MinValue, sp, "Succesfull Quandl prices import", 1, null);
                }
                catch (Exception ex)
                {
                    logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, sp, "Unsuccesfull Quandl prices import", 0, ex);
                }
            }
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

    }
}
