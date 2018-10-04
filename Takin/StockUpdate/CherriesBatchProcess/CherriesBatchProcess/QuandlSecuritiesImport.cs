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
    public class QuandlSecuritiesImport
    {

        public static void Execute(Dictionary<string, string> config)   
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
            ftpServerUrl = config[Constants.QuandlSecuritiesFTPServerURL] + config[Constants.QuandlAPIkey];
            ftpUserName = config[Constants.QuandlSecuritiesFTPUserName];
            ftpPassword = config[Constants.QuandlSecuritiesFTPPassword];
            doDeleteFiles = config[Constants.DoDeleteFiles];

            using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString_Quandl].ToString()))
            {
                conn.Open();
                Logger logger = new Logger(conn, Constants.QuandlSecuritiesImport);

                //string httpfilename = "https://www.quandl.com/api/v3/datatables/ZACKS/CP.csv?api_key=ULxnhY2CBervENYkyNri";
                string filename = null;
                string fileContent = null;

                try
                {
                    filename = "INTRINIO_Securities_initial.txt";
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

                    logger.WriteLog(Constants.Success, filename, DateTime.MinValue, null, "Succesfull QUANDL securities FTP file import", 1, null);
                }
                catch (Exception ex)
                {
                    logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, null, "Unsuccesfull QUANDL securities FTP file import", 0, ex);
                    return;
                }

                string sp = null;
                try
                {
                    DataTable importedSecuritiesTable = getSecTableStructure();

                    string[] securityRecords = fileContent.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    int i = 0;
                    // loop by imported securities
                    foreach (string securityRec in securityRecords)
                    {
                        i++;
                        if (i == 1) continue;

                        //string[] security = securityRec.Split(new char[] { ',' });
                        string[] security = getSplitString(securityRec);        //securityRec.Split(new char[] { '"', '\"' });

                        DataRow importedSecuruty = importedSecuritiesTable.NewRow();
                        getSecurityRow(importedSecuruty, securityRec);
                        importedSecuritiesTable.Rows.Add(importedSecuruty);
                        importedSecuritiesTable.AcceptChanges();
                    }

                    sp = Constants.importQuandlSecurities;
                    SqlCommand securityImportCommand = new SqlCommand(String.Empty, conn);
                    securityImportCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    securityImportCommand.CommandText = sp;
                    securityImportCommand.CommandTimeout = Constants.SPsTimeout;
                    securityImportCommand.Parameters.Add("@imported_secs", SqlDbType.Structured).Value = importedSecuritiesTable;
                    securityImportCommand.ExecuteNonQuery();

                    sp = null;
                    string sourceFileFullName = clientFolder + "\\" + filename;
                    string processedFileFullName = processedFolder + "\\" + filename;

                    if (doDeleteFiles == "0")
                        File.WriteAllText(processedFileFullName, fileContent);

                    File.Delete(sourceFileFullName);

                    sp = Constants.importEODSecuritiesImport;
                    logger.WriteLog(Constants.Success, filename, DateTime.MinValue, sp, "Succesfull Quandl securities import", 1, null);
                }
                catch (Exception ex)
                {
                    logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, sp, "Unsuccesfull Quandl securities import", 0, ex);
                }
            }
        }

        private static void getSecurityRow(DataRow importedSecuruty, string securityRec)
        {
            try
            {
                //string[] security = securityRec.Split(new char[] { ',' });
                string[] security = getSplitString(securityRec);        //securityRec.Split(new char[] { '"', '\"' });

                importedSecuruty["symbol"] = security[1];
                importedSecuruty["sec_name"] = security[3];
                importedSecuruty["exchange"] = security[4];
                importedSecuruty["currency_code"] = security[5];
                importedSecuruty["description"] = security[6];
                importedSecuruty["address"] = security[7];
                importedSecuruty["city"] = security[9];
                importedSecuruty["state"] = security[10];
                importedSecuruty["country_code"] = security[11];
                importedSecuruty["post_code"] = security[12];
                importedSecuruty["phone"] = security[13];
                importedSecuruty["fax"] = security[14];
                importedSecuruty["email"] = security[15];
                importedSecuruty["website"] = security[16];
                importedSecuruty["sector_code"] = security[23];
                importedSecuruty["sector_desc"] = security[24];
                importedSecuruty["industry_code"] = security[25];
                importedSecuruty["industry_desc"] = security[26];
                //return importedSecuruty;
            }
            catch (Exception ex)
            {
            }
        }

        private static DataTable getSecTableStructure()
        {
            DataTable importedSecuritiesTable = new DataTable();
            try
            {
                importedSecuritiesTable.Columns.Add("symbol");
                importedSecuritiesTable.Columns.Add("sec_name");
                importedSecuritiesTable.Columns.Add("exchange");
                importedSecuritiesTable.Columns.Add("currency_code");
                importedSecuritiesTable.Columns.Add("description");
                importedSecuritiesTable.Columns.Add("address");
                importedSecuritiesTable.Columns.Add("city");
                importedSecuritiesTable.Columns.Add("state");
                importedSecuritiesTable.Columns.Add("country_code");
                importedSecuritiesTable.Columns.Add("post_code");
                importedSecuritiesTable.Columns.Add("phone");
                importedSecuritiesTable.Columns.Add("fax");
                importedSecuritiesTable.Columns.Add("email");
                importedSecuritiesTable.Columns.Add("website");
                importedSecuritiesTable.Columns.Add("sector_code");
                importedSecuritiesTable.Columns.Add("sector_desc");
                importedSecuritiesTable.Columns.Add("industry_code");
                importedSecuritiesTable.Columns.Add("industry_desc");
            }
            catch (Exception ex)
            {
            }
            return importedSecuritiesTable;
        }

        private static string[] getSplitString(string securityRec)
        {
            List<string> security = new List<string>();
            
            //string[] security = new string[] {"a"};

            int indComa;
            int indQuotation = 0;
            int i = 0;
            string subStrComa = "";
            string subStrQuotation = "";

            try
            {
                indComa = securityRec.IndexOf(',');

                while (indComa >= 0 || securityRec.Substring(0, 1) == "\"")
                {
                    if (securityRec.Substring(0, 1) != "\"")
                    {
                        subStrComa = securityRec.Substring(0, indComa);

                        //security[i] = subStrComa;
                        security.Add(subStrComa);
                        i++;
                        securityRec = securityRec.Substring(indComa + 1);
                        indComa = securityRec.IndexOf(',');
                    }
                    else
                    {
                        // case when first character is '"', find next '"' and
                        indQuotation = securityRec.IndexOf('"', 1 );
                        subStrQuotation = securityRec.Substring(1, indQuotation - 1);
                        //security[i] = subStrQuotation;
                        security.Add(subStrQuotation);
                        i++;
                        securityRec = securityRec.Substring(indQuotation + 1);
                        if (securityRec.Substring(0, 1) == ",")
                            securityRec = securityRec.Substring(1);

                        indComa = securityRec.IndexOf(',');

                    }
                }

                if(indComa < 0 && securityRec.Length > 0)
                    security.Add(securityRec);
            }
            catch (Exception ex)
            {
            }

            return security.ToArray();
        }
    }
}
