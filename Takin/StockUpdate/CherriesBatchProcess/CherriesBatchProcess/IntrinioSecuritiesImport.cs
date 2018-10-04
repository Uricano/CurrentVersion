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
    public class IntrinioSecuritiesImport
    {
        public static void Execute(Dictionary<string, string> config, int[] exchangeID)
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
            ftpServerUrl = config[Constants.IntrinioSecuritiesFTPServerURL_start] + "&" + config[Constants.IntrinioSecuritiesFTPServerURL_end];
            ftpUserName = config[Constants.IntrinioSecuritiesFTPUserName];
            ftpPassword = config[Constants.IntrinioSecuritiesFTPPassword];
            string fullUrl;
            string strSymbols = "";
            int counter12 = 0;
            doDeleteFiles = config[Constants.DoDeleteFiles];

            using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString_Quandl].ToString()))
            {
                conn.Open();
                Logger logger = new Logger(conn, Constants.IntrinioSecuritiesImport);

                string filename = null;
                string fileContent = null;

                // Get securities
                //TODO: WARNING!!! Change to update by exchanges and not only by SYMBOLS! HOW???  :US/:IT???

                string query = "SELECT  strSymbol FROM tbl_Securities where idSecurity like '1.%' and strDescription is null and strAddress is null order by strSymbol";

                //////string query = "SELECT  strSymbol FROM tbl_Securities where idsecurity in ('1.10004','1.10019','1.10026','1.10044')";

                //////string query = "SELECT  strSymbol FROM tbl_Securities where idsector is null order by strSymbol";

                DataTable dtSecurities = getDBTable(query, conn);

                for (int s = 0; s < dtSecurities.Rows.Count; s++)
                {
                    //id_exchange = dtIntriSymbol.Rows[i]["idStockExchange"].ToString();
                    //intri_symbol = dtIntriSymbol.Rows[i]["intrinioSymbol"].ToString();

                    strSymbols = strSymbols + "," + dtSecurities.Rows[s]["strSymbol"].ToString();
                    counter12++;
                    if (counter12 == 10 || s == dtSecurities.Rows.Count - 1)
                    {

                        strSymbols = strSymbols.Substring(1); //throw 1 comma
                        // Table for 12 securities
                        fullUrl = string.Format(ftpServerUrl, strSymbols);

                        try
                        {
                            filename = string.Format("INTRINIO_Securities_{0}.txt", s+1);
                            //string fullUrl = ftpServerUrl;
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

                            logger.WriteLog(Constants.Success, filename, DateTime.MinValue, strSymbols, "Succesfull Intrinio securities FTP file import " + (s + 1).ToString(), 1, null);
                        }
                        catch (Exception ex)
                        {
                            logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, strSymbols, "Unsuccesfull Intrinio securities FTP file import " + (s + 1).ToString(), 0, ex);
                            //return;
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
                                if (i < 3) continue;

                                //string[] security = securityRec.Split(new char[] { ',' });
                                string[] security = getSplitString(securityRec);        //securityRec.Split(new char[] { '"', '\"' });

                                DataRow importedSecuruty = importedSecuritiesTable.NewRow();
                                if (getSecurityRow(importedSecuruty, securityRec))   //;      //***getSecurityRow(...
                                {
                                    importedSecuritiesTable.Rows.Add(importedSecuruty);
                                    importedSecuritiesTable.AcceptChanges();
                                }
                            }

                            sp = Constants.importIntrinioSecurities;
                            SqlCommand securityImportCommand = new SqlCommand(String.Empty, conn);
                            securityImportCommand.CommandType = System.Data.CommandType.StoredProcedure;
                            securityImportCommand.CommandText = sp;
                            securityImportCommand.CommandTimeout = Constants.SPsTimeout;
                            securityImportCommand.Parameters.Add("@imported_secs", SqlDbType.Structured).Value = importedSecuritiesTable;
                            securityImportCommand.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = string.Join(", ", exchangeID);    // id_exchange; //TODO: CHANGE LATER when know what to do with symbols in 2 exchanges
                            securityImportCommand.ExecuteNonQuery();

                            //sp = null;
                            string sourceFileFullName = clientFolder + "\\" + filename;
                            string processedFileFullName = processedFolder + "\\" + filename;

                            if (doDeleteFiles == "0")
                                File.WriteAllText(processedFileFullName, fileContent);

                            File.Delete(sourceFileFullName);

                            //sp = Constants.importEODSecuritiesImport;
                            logger.WriteLog(Constants.Success, filename, DateTime.MinValue, sp, "Succesfull Intrinio securities import " + (s+1).ToString(), 1, null);
                        }
                        catch (Exception ex)
                        {
                            logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, sp, "Unsuccesfull Intrinio securities import " + (s + 1).ToString(), 0, ex);
                        }

                        counter12 = 0;
                        strSymbols = "";

                    }// if of FOR

                }// for
            }// using
        }

        private static void getSecurityInitRow(DataRow importedSecuruty, string securityRec)
        {
            try
            {
                //string[] security = securityRec.Split(new char[] { ',' });
                string[] security = getSplitString(securityRec);        //securityRec.Split(new char[] { '"', '\"' });

                importedSecuruty["symbol"] = security[0];
                importedSecuruty["sec_name"] = security[1];
            }
            catch (Exception ex)
            {
            }
        }


        private static bool getSecurityRow(DataRow importedSecuruty, string securityRec)
        {
            try
            {
                string temp;
                //string[] security = securityRec.Split(new char[] { ',' });
                string[] security = getSplitString(securityRec);        //securityRec.Split(new char[] { '"', '\"' });

                importedSecuruty["symbol"] = security[0];
                //importedSecuruty["sec_name"] = security[2];
                //importedSecuruty["security_type"] = security[3];
                //importedSecuruty["currency_code"] = "9001"; //TODO:  change later
                temp = security[2].Trim();
                if (temp != "na" && temp != "nm" && temp != "N/A")
                    importedSecuruty["business_address"] = temp;
                temp = security[3].Trim();
                if (temp != "na" && temp != "nm" && temp != "N/A")
                    importedSecuruty["business_phone_no"] = temp;
                temp = security[4].Trim();
                if (temp != "na" && temp != "nm" && temp != "N/A")
                    importedSecuruty["company_url"] = temp;
                temp = security[5].Trim();
                if (temp != "na" && temp != "nm" && temp != "N/A")
                    importedSecuruty["ceo"] = temp;
                importedSecuruty["country"] = security[6];
                importedSecuruty["state"] = security[7];
                importedSecuruty["industry_category"] = security[8];
                importedSecuruty["sector"] = security[9];
                //importedSecuruty["stock_exchange"] = security[12];
                importedSecuruty["beta"] = security[10];
                temp = security[11].Trim();
                if (temp != "na" && temp != "nm" && temp != "N/A")
                    importedSecuruty["long_description"] = temp;
                importedSecuruty["cik"] = security[12];
                importedSecuruty["figi"] = security[14];

                ////if (security[14] != "") // In SP "importIntrinioSecurities" securities are updated by strSYMBOL and FIGI
                if (security[0] != "") // In SP "importIntrinioSecurities" securities are updated by strSYMBOL  and USCOMP only
                        return true;
                else
                    return false;


                ////***************  NOT RECEIVING GOOD DATA FOR THIS FIELD  **********
                //temp = security[13].Trim().ToUpper();
                //if (temp != "na" && temp != "nm" && temp != "N/A")
                //{
                //    if (temp == "NYSE MKT")
                //        temp = "AMEX";
                //    else if (temp.Length >= 6 && temp.Substring(0, 6) == "NASDAQ")
                //        temp = "NASDAQ";
                //    // there is also just 'NYSE'

                //    if (temp == "AMEX" || temp == "NYSE" || temp == "NASDAQ")
                //    {
                //        importedSecuruty["intrinio_exchange"] = temp;
                //        return true;
                //    }
                //    else
                //        return false;
                //}
                //else
                //    return false;
                ////**************************************************************************


            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static DataTable getSecInitTableStructure()
        {
            DataTable importedSecuritiesTable = new DataTable();
            try
            {
                importedSecuritiesTable.Columns.Add("symbol");
                importedSecuritiesTable.Columns.Add("sec_name");
            }
            catch (Exception ex)
            {
            }
            return importedSecuritiesTable;
        }

        private static DataTable getSecTableStructure()
        {

            //ticker,name,security_type,business_address,business_phone_no,company_url,ceo,country,mailing_address,industry_category,sector,stock_exchange,beta,long_description,cik
            DataTable importedSecuritiesTable = new DataTable();
            try
            {
                importedSecuritiesTable.Columns.Add("symbol");
                //importedSecuritiesTable.Columns.Add("sec_name");
                //importedSecuritiesTable.Columns.Add("security_type");
                //importedSecuritiesTable.Columns.Add("currency_code");
                importedSecuritiesTable.Columns.Add("business_address");
                importedSecuritiesTable.Columns.Add("business_phone_no");
                importedSecuritiesTable.Columns.Add("company_url");
                importedSecuritiesTable.Columns.Add("ceo");
                importedSecuritiesTable.Columns.Add("country");
                importedSecuritiesTable.Columns.Add("state");
                importedSecuritiesTable.Columns.Add("industry_category");
                importedSecuritiesTable.Columns.Add("sector");
                //importedSecuritiesTable.Columns.Add("stock_exchange");
                importedSecuritiesTable.Columns.Add("beta");
                importedSecuritiesTable.Columns.Add("long_description");
                importedSecuritiesTable.Columns.Add("cik");
                importedSecuritiesTable.Columns.Add("intrinio_exchange");
                importedSecuritiesTable.Columns.Add("figi");
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
                        indQuotation = securityRec.IndexOf('"', 1);
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

                if (indComa < 0 && securityRec.Length > 0)
                    security.Add(securityRec);
            }
            catch (Exception ex)
            {
            }

            return security.ToArray();
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
