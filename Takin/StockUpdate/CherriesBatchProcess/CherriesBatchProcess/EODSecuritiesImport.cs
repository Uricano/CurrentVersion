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
    public class EODSecuritiesImport 
    {
        public static void Execute(Dictionary<string, string> config, string[] exchanges)
        {
            string clientFolder = null;
            string processedFolder = null;
            string rejectedFolder = null;
            string ftpServerUrl = null;
            string ftpUserName = null;
            string ftpPassword = null;
            string doDeleteFiles = null;
 
            clientFolder = config[Constants.SourceDir];
            processedFolder = config[Constants.ProcessedDir];
            rejectedFolder = config[Constants.RejectedDir];
            ftpServerUrl = config[Constants.EODSecuritiesFTPServerURL];
            ftpUserName = config[Constants.EODSecuritiesFTPUserName];
            ftpPassword = config[Constants.EODSecuritiesFTPPassword];
            doDeleteFiles = config[Constants.DoDeleteFiles];

            using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString].ToString()))
            {
                conn.Open();
                Logger logger = new Logger(conn, Constants.EODSecuritiesImport);

                foreach (string exchange in exchanges)
                {
                    string filename = null;
                    string fileContent = null;

                    try
                    {
                        filename = exchange + ".txt";
                        string fullUrl = ftpServerUrl + "/" + filename;
                        string fullFilename = clientFolder + "\\" + filename;
                        FtpWebRequest requestf = (FtpWebRequest)WebRequest.Create(fullUrl);
                        requestf.KeepAlive = false;
                        requestf.Method = WebRequestMethods.Ftp.DownloadFile;
                        requestf.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                        FtpWebResponse respf = (FtpWebResponse)requestf.GetResponse();
                        Stream respfStream = respf.GetResponseStream();
                        StreamReader readerf = new StreamReader(respfStream);
                        fileContent = readerf.ReadToEnd();
                        readerf.Close();
                        respf.Close();
                        if (!Directory.Exists(clientFolder)) Directory.CreateDirectory(clientFolder);
                        File.WriteAllText(fullFilename, fileContent);

                        logger.WriteLog(Constants.Success, filename, DateTime.MinValue, null, "Succesfull FTP file import", 1, null);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, null, "Unsuccesfull FTP file import", 0, ex);
                        continue;
                    }

                    // TODO: Delete imported files


                    string sp = null;
                    try
                    {
                        DataTable importedSecuritiesTable = new DataTable();
                        importedSecuritiesTable.Columns.Add("symbol");
                        importedSecuritiesTable.Columns.Add("sec_name");
                        importedSecuritiesTable.Columns.Add("sector");

                        string[] securityRecords = fileContent.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        int i = 0;
                        // loop by imported securities
                        foreach (string securityRec in securityRecords)
                        {
                            i++;
                            if (i == 1) continue;

                            string[] security = securityRec.Split(new char[] { '\t' });

                            DataRow importedSecuruty = importedSecuritiesTable.NewRow();
                            importedSecuruty["symbol"] = security[0];
                            importedSecuruty["sec_name"] = security[1];
                            importedSecuruty["sector"] = security[2];

                            importedSecuritiesTable.Rows.Add(importedSecuruty);
                            importedSecuritiesTable.AcceptChanges();
                        }

                        sp = Constants.importEODSecuritiesImport;
                        SqlCommand securityImportCommand = new SqlCommand(String.Empty, conn);
                        securityImportCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        securityImportCommand.CommandText = sp;
                        securityImportCommand.CommandTimeout = Constants.SPsTimeout;
                        securityImportCommand.Parameters.Add("@exchange", SqlDbType.VarChar, 20).Value = exchange;
                        securityImportCommand.Parameters.Add("@imported_secs", SqlDbType.Structured).Value = importedSecuritiesTable;
                        securityImportCommand.ExecuteNonQuery();

                        sp = null;
                        string sourceFileFullName = clientFolder + "\\" + filename;
                        string processedFileFullName = processedFolder + "\\" + filename;

                        if (doDeleteFiles == "0")
                          File.WriteAllText(processedFileFullName, fileContent);

                        File.Delete(sourceFileFullName);

                        sp = Constants.importEODSecuritiesImport;
                        logger.WriteLog(Constants.Success, filename, DateTime.MinValue, sp, "Succesfull securities import", 1, null);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, sp, "Unsuccesfull securities import", 0, ex);
                    }
                }
            }
        }
    }
}
