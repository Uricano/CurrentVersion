/*
 * EOD data import
 * 
 * args:
 * -daily -p
 * -daily -i
 * -last -p
 * -last -i
 * 2016/01/01 2016/12/31 -p
 * 2016/01/01 2016/12/31 -i
 * 
 * -daily  today and yesterday data
 * -last   last date data
 * startDate endDate dates interval data
 * -p prices
 * -i indeces
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace CherriesBatchProcess
{
    public class EODPricesImport
    {
        public static void Execute(string[] args, Dictionary<string, string> config, string[] exchanges, int indexes_exchange_id)
        {
            DateTime startDateInitial = DateTime.MinValue;
            DateTime endDateInitial = DateTime.MaxValue; 
            string clientFolder = null;
            string processedFolder = null;
            string rejectedFolder = null;
            string ftpServerUrl = null;
            string ftpUserName = null;
            string ftpPassword = null;
            bool importPrices = false;
            bool importIndexes = false;
            string doDeleteFiles = null;


            clientFolder = config[Constants.SourceDir];
            processedFolder = config[Constants.ProcessedDir];
            rejectedFolder = config[Constants.RejectedDir];
            ftpServerUrl = config[Constants.EODPricesFTPServerURL];
            ftpUserName = config[Constants.EODPricesFTPUserName];
            ftpPassword = config[Constants.EODPricesFTPPassword];
            doDeleteFiles = config[Constants.DoDeleteFiles];

            using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString]))
            {
                Logger logger = null;
                
                conn.Open();

                try
                {
                    if (args[0].ToLower() == Constants.Daily)
                    {
                        logger = new Logger(conn, Constants.EODPricesImport);
                        startDateInitial = DateTime.Today.AddDays(-1);
                        endDateInitial = DateTime.Today;
                    }
                    else if (args[0].ToLower() == Constants.Last)
                    {
                        logger = new Logger(conn, Constants.EODPricesImport);
                    }
                    else
                    {
                        logger = new Logger(conn, Constants.EODHistPricesImport);
                        startDateInitial = DateTime.ParseExact(args[0], Constants.DateParmsFormat, System.Globalization.CultureInfo.CurrentCulture);
                        endDateInitial = DateTime.ParseExact(args[1], Constants.DateParmsFormat, System.Globalization.CultureInfo.CurrentCulture);
                    }

                    foreach (string arg in args)
                    {
                        if (arg.ToLower() == Constants.Prices) importPrices = true;
                        if (arg.ToLower() == Constants.Indeces) importIndexes = true;
                    }
                    if (importPrices == false && importIndexes == false) throw new Exception("Specify prices or indexes import");
                }
                catch (Exception ex)
                {
                    logger.WriteLog(Constants.Failure, null, DateTime.MinValue, null, "Accept command-line parameters", 0, ex);
                    return;
                }

                string[] files;

                try
                {
                    FtpWebRequest requestd = (FtpWebRequest)WebRequest.Create(ftpServerUrl);
                    requestd.KeepAlive = false;
                    requestd.Method = WebRequestMethods.Ftp.ListDirectory;
                    requestd.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                    FtpWebResponse respd = (FtpWebResponse)requestd.GetResponse();
                    Stream respdStream = respd.GetResponseStream();
                    StreamReader readerd = new StreamReader(respdStream);
                    files = readerd.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    readerd.Close();
                    respd.Close();

                    logger.WriteLog(Constants.Success, clientFolder, DateTime.MinValue, null, "List of directory receiving", 0, null);
                }
                catch (Exception ex)
                {
                    logger.WriteLog(Constants.Failure, clientFolder, DateTime.MinValue, null, "List of directory receiving", 0, ex);
                    return;
                }

                foreach (string exchange in exchanges)
                {
                    DateTime startDate = startDateInitial;
                    DateTime endDate = endDateInitial;
                    List<string> selectedFiles = new List<string>();

                    // Reset date range based on files selected
                    if (args[0].ToLower() == Constants.Last)
                    {
                        DateTime lastDate = DateTime.MinValue;
                        foreach (string filename in files)
                        {
                            if (CheckFilename(filename, exchanges) == false) continue;
                            if ((GetExchangeFromFilename(filename) == exchange) && (GetDateFromFilename(filename) > lastDate)) lastDate = GetDateFromFilename(filename);
                        }

                        if (lastDate == DateTime.MinValue) continue;

                        startDate = lastDate.AddDays(-1);
                        endDate = lastDate;
                    }

                    // Build list of file NAMES for each EXCHANGE separately
                    foreach (string filename in files)
                    {
                        if (CheckFilename(filename, exchanges) == false) continue;
                        if ((GetExchangeFromFilename(filename) == exchange) && (GetDateFromFilename(filename) >= startDate) && (GetDateFromFilename(filename) <= endDate))
                            selectedFiles.Add(filename);
                    }


                    //////---------------  OR  --- (For running with files already in 'clientFolder')--(TEMPORARY, DO NOT DELETE!!!)---------
                    ////if (Directory.Exists(clientFolder))
                    ////{
                    ////    //Directory.Delete(tempFolder, true);
                    ////    string[] exchangeFiles = Directory.GetFiles(clientFolder);
                    ////    for (int jj = 0; jj < exchangeFiles.Length; jj++)
                    ////    {
                    ////        if (exchangeFiles[jj].Contains(exchange))
                    ////            selectedFiles.Add(exchangeFiles[jj].Substring(exchangeFiles[jj].LastIndexOf("\\")+1));
                    ////    }
                    ////}
  

                    string sp = null;

                    // DELETE prices in given price range, prior to writing new ones
                    try
                    {
                        if (importPrices == true)
                        {
                            sp = Constants.importEODCleanDataForEODImport;
                            SqlCommand cleanDataSP = new SqlCommand(string.Empty, conn);
                            cleanDataSP.CommandType = System.Data.CommandType.StoredProcedure;
                            cleanDataSP.CommandText = sp;
                            cleanDataSP.CommandTimeout = Constants.SPsTimeout;
                            cleanDataSP.Parameters.Add("@exchange", SqlDbType.VarChar, 20).Value = exchange;    // is it Symbol???
                            cleanDataSP.Parameters.Add("@dates_seq", SqlDbType.VarChar, 20).Value = "RANGE";
                            cleanDataSP.Parameters.Add("@start_date", SqlDbType.DateTime).Value = startDate;
                            cleanDataSP.Parameters.Add("@end_date", SqlDbType.DateTime).Value = endDate;
                            cleanDataSP.ExecuteNonQuery();
                            logger.WriteLog(Constants.Success, exchange, DateTime.MinValue, sp, "Succesfull table data cleaning", 0, null);
                        }

                        if (importIndexes == true)
                        {
                            sp = Constants.importCleanDataForEODIndexesImport;
                            SqlCommand cleanDataSP = new SqlCommand(string.Empty, conn);
                            cleanDataSP.CommandType = System.Data.CommandType.StoredProcedure;
                            cleanDataSP.CommandText = sp;
                            cleanDataSP.CommandTimeout = Constants.SPsTimeout;
                            cleanDataSP.Parameters.Add("@id_exchange", SqlDbType.Int).Value = indexes_exchange_id;
                            cleanDataSP.Parameters.Add("@dates_seq", SqlDbType.VarChar, 20).Value = "RANGE";
                            cleanDataSP.Parameters.Add("@start_date", SqlDbType.DateTime).Value = startDate;
                            cleanDataSP.Parameters.Add("@end_date", SqlDbType.DateTime).Value = endDate;
                            cleanDataSP.ExecuteNonQuery();
                            logger.WriteLog(Constants.Success, exchange, DateTime.MinValue, sp, "Succesfull table data cleaning", 0, null);
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, exchange, DateTime.MinValue, sp, "Unsuccessfull table data cleaning", 0, ex);
                        continue;
                    }

                    EODFilenamesComparer filenameComparer = new EODFilenamesComparer();
                    selectedFiles.Sort(filenameComparer);

                    foreach (string filename in selectedFiles)
                    {
                        // Download text files to 'clientFolder' via FTP
                        try
                        {
                            string fullUrl = ftpServerUrl + "/" + filename;
                            string fullFilename = clientFolder + "\\" + filename;
                            FtpWebRequest requestf = (FtpWebRequest)WebRequest.Create(fullUrl);
                            requestf.KeepAlive = false;
                            requestf.Method = WebRequestMethods.Ftp.DownloadFile;
                            requestf.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                            FtpWebResponse respf = (FtpWebResponse)requestf.GetResponse();
                            Stream respfStream = respf.GetResponseStream();
                            StreamReader readerf = new StreamReader(respfStream);
                            string fileContent = readerf.ReadToEnd();
                            readerf.Close();
                            respf.Close();
                            if (!Directory.Exists(clientFolder)) Directory.CreateDirectory(clientFolder);
                            File.WriteAllText(fullFilename, fileContent);

                            logger.WriteLog(Constants.Success, filename, GetDateFromFilename(filename), null, "Succesfull FTP csv file import", 1, null);
                        }
                        catch (Exception ex)
                        {
                            logger.WriteLog(Constants.Error, filename, GetDateFromFilename(filename), null, "Unsuccessfull FTP csv file import", 0, ex);
                            continue;
                        }
           
                        string spi = null;
                        try
                        {
                            if (importPrices == true)
                            {
                                spi = Constants.importEODImport; 
                                SqlCommand importSP = new SqlCommand(string.Empty, conn);
                                importSP.CommandType = System.Data.CommandType.StoredProcedure;
                                importSP.CommandText = spi;



                                importSP.CommandTimeout = Constants.SPsTimeout;
                                DataTable dt = GetPriceData(clientFolder + "\\" + filename);
                                importSP.Parameters.Add("@EOD_file_data", SqlDbType.Structured).Value = dt; //clientFolder + "\\" + filename;
                                importSP.Parameters.Add("@exchange", SqlDbType.VarChar, 20).Value = GetExchangeFromFilename(filename);
                                importSP.Parameters.Add("@date", SqlDbType.DateTime).Value = GetDateFromFilename(filename);
                                importSP.ExecuteNonQuery();
                                logger.WriteLog(Constants.Success, filename, GetDateFromFilename(filename), spi, "Succesfull security prices data import", 1, null);
                            }

                            if (importIndexes == true)
                            {
                                spi = Constants.importEODIndexes; 
                                SqlCommand importSP = new SqlCommand(string.Empty, conn);
                                importSP.CommandType = System.Data.CommandType.StoredProcedure;
                                importSP.CommandText = spi;
                                importSP.CommandTimeout = Constants.SPsTimeout;
                                DataTable dt = GetPriceData(clientFolder + "\\" + filename);
                                importSP.Parameters.Add("@EOD_file_data", SqlDbType.Structured).Value = dt;
                                importSP.Parameters.Add("@exg_code", SqlDbType.Int).Value = indexes_exchange_id;
                                importSP.Parameters.Add("@date", SqlDbType.DateTime).Value = GetDateFromFilename(filename);
                                importSP.ExecuteNonQuery();
                                logger.WriteLog(Constants.Success, filename, GetDateFromFilename(filename), spi, "Succesfull index prices data import", 1, null);
                            }

                            spi = null;
                            if (doDeleteFiles == "0")
                               MoveFile(clientFolder + "\\" + filename, processedFolder + "\\" + filename);
                        }
                        catch (Exception ex)
                        {
                            if (doDeleteFiles == "0")
                                MoveFile(clientFolder + "\\" + filename, rejectedFolder + "\\" + filename);

                            logger.WriteLog(Constants.Error, filename, GetDateFromFilename(filename), spi, "Unsuccesfull security/index prices table data import", 0, ex);
                        }
                        //TODO: Delete file in ClientFolder
                        if (doDeleteFiles == "1")
                           File.Delete(clientFolder + "\\" + filename);
                    }

                    // TODO: Update tblSel_StockExchanges.LastUpdate for proper Exchange for Today
                    if (importIndexes == true)
                        GeneralFunctions.SetLastUpdateDate(conn, indexes_exchange_id);      //LR    Q1: do we have to update it if not successfull ?? 

                    if (importPrices == true)
                        GeneralFunctions.SetLastUpdateDate(conn, exchange);                 //LR    Q1: do we have to update it if not successfull ?? 

                }// foreach(string exchange............)
            }
        }

        private static DataTable GetPriceData(string fileName)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("symbol", typeof(string)));
            dt.Columns.Add(new DataColumn("[date]", typeof(string)));
            dt.Columns.Add(new DataColumn("[open]", typeof(string)));
            dt.Columns.Add(new DataColumn("high", typeof(string)));
            dt.Columns.Add(new DataColumn("low", typeof(string)));
            dt.Columns.Add(new DataColumn("[close]", typeof(string)));
            dt.Columns.Add(new DataColumn("volume", typeof(string)));
            var sr = new StreamReader(fileName);
            string line = "";
            do
            {
                DataRow row = dt.NewRow();
                line = sr.ReadLine();
                string[] itemArray = line.Split(',');
                row.ItemArray = itemArray;
                dt.Rows.Add(row);
            } while (!sr.EndOfStream && !string.IsNullOrEmpty(line));
            sr.Close();
            return dt;
        }

        private static bool CheckFilename(string filename, string[] exchanges)
        {
            int i = filename.IndexOf("_");
            if (i < 0) return false;
            if (new List<string>(exchanges).Contains(filename.Substring(0, i))) return true; 
            return false;
        }

        private static string GetExchangeFromFilename(string filename)
        {
            int i = filename.IndexOf("_");
            return filename.Substring(0, i);
        }

        private static DateTime GetDateFromFilename(string filename)
        {
            int i = filename.IndexOf("_") + 1;
            return DateTime.ParseExact(
                filename.Substring(i, Constants.EODDateInFilenameFormat.Length), 
                Constants.EODDateInFilenameFormat, System.Globalization.CultureInfo.CurrentCulture);
        }

        private static void MoveFile (string sourceFileFullName, string targetFileFullName)
        {
            string fileContent = File.ReadAllText(sourceFileFullName);
            File.WriteAllText(targetFileFullName, fileContent);
            File.Delete(sourceFileFullName);
        }

        private class EODFilenamesComparer : IComparer<string>
        {
            public int Compare(string filename1, string filename2)
            {
                return GetDateFromFilename(filename1).CompareTo(GetDateFromFilename(filename2));
            }
        }
    }
}
