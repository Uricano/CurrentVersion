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
using System.Threading;

namespace CherriesBatchProcess
{
    public class IntrinioHistPricesImport
    {
        public static void Execute(Dictionary<string, string> config, int[] exchangeID, List<string> secWithDividOrSplit, bool isTASE = false, bool isNewSecurities = false)
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
            bool isDividentsSplits = false;
            DateTime importDate = DateTime.Today.AddDays(-1);

            bool isPricesForNewSecurities = false;
            List<string> secImportHistorical = new List<string>();

            clientFolder = config[Constants.SourceDir];
            processedFolder = config[Constants.ProcessedDir];
            rejectedFolder = config[Constants.RejectedDir];
            ftpServerUrlstart = config[Constants.IntrinioPricesFTPServerURL];
            //ftpServerUrl = string.Format("{0}{1}&{2}", ftpServerUrlstart, "AAPL", config[Constants.QuandlAPIkey]);
            ftpUserName = config[Constants.IntrinioSecuritiesFTPUserName];
            ftpPassword = config[Constants.IntrinioSecuritiesFTPPassword];
            string fullUrl;
            doDeleteFiles = config[Constants.DoDeleteFiles];

            // Historical prices
            if (isTASE)
            {
                intriMediaURL = @"https://api.intrinio.com/prices.csv?ticker={0}:IT";
                shekToAgorot = 100.0;
            }
            else
            {
                intriMediaURL = @"https://api.intrinio.com/prices.csv?ticker={0}:US";
                shekToAgorot = 1.0;
            }

            using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString_Quandl].ToString()))
            {
                conn.Open();
                Logger logger = new Logger(conn, Constants.IntrinioPricesImport);

                bool completeMissingPricesFillingSuccess = false;
                bool duplicateLastDayPricesSuccess = false;
                bool calcPriceReturnsSuccess = false;
                bool setIsTradedSuccess = false;

                string sp = null;

                string filename = null;
                string fileContent = null;
                string sec_symbol;

                //"IBI,ICL,IGLD,ININ,ISTA,ITRN,JCDA,KEN,KING,KMDA,KTOV,LBRT,LIFE,LPSN,MAGS,MAIN,MCC,MDGS,MGIC,MINI,MLNX,MNKD,MRIN,MTRN,MTRX,MYL,MYSZ,MZOR,NAVB,NDLS,NICE,NNDM,NSPR,NTEC,NTGR,NTS,NVMI,NXTM,OBAS,ONE,OPK,ORA,PBTH,PEN,PERI,PFLT,PIH,PLX,PNTR,PRGO,PTNR,RDHL,RTLX,RVSN,SAFE,SILC,SLGN,SMT,SODA,SPNS,STRS,SYNO,TAT,TEVA,TLRD,TOPS,TRPX,TSEM,ULTR,UNIT,VISN"

                if (secWithDividOrSplit.Count < 1)
                {
                    // Get securities - CUSTOM code
                    //string strCustom = "'BHO','BEXP','LSTZA','MNLO','APLS','FRZ','PPDI','DHC','GEC','IES','BCON','PPDF','NSI','CLNC','PSAV','WELL','SVI','USFP','HOL','GFLY'";
                    //string strCustom = "'TMX','ARS','GFIS','AIRI','PRISB','STBC','CLFC','GNTY','SNS','PLC','VIR','GTES','WCNX','DESP','PMAR','LGZ','DCPH','LAC','FSN'";
                    //string strCustom = "'BPMX','ADSI','ADIL','MOG A','PRS','WHSM','BNHNA','YTI','ANK','DSG','MTLp','CRW','EYPT','BTG','PCC','BFF','ABP','ECP','HUD','SYMS','PNL','DPTR','PMIC','AVST'";
                    //string strCustom = "'PNL','HJLI','BAS','TTP','ABVC','EEE','GPRC','SWCH','DMI','BQI','JOYG','REI','FNBG','CEU','LRP','ENSV','BB','AUG','MLSS','MBP','ASXWI','ABK','IOP','KWIC','BCBPR','PRSP','AQUA','AIX','MCB','DHCP','FMSPR'";
                    //string strCustom = "'NINE','ILPT','NKRC','AIP','MF','JBX','PXT','AGS','CAAP','UAGR','LLGX','NTI','CK','NASB','FNC','VMET','SCHK','ARSD','SMTS','DOR','TPHS','MTWS','VICI','ANTE','MBR','DOR','ADLR','EHR','AKC','CURO'";
                    //string strCustom = "'LMCA','FFH','LCRT','SWB','CAHS','ENP','TOH','CRHM','MOTA','PAET','HAF','INXI','GHQ','FHC','SOK','AVI','PMG','FIZ','MMA','VCC','TPGH','UTA','FTSI','DROOY','FNDT','GECX','WRP','BARI','CIZ','NXG','VSEA'";
                    //string strCustom = "'FFH','CANF','CASA','ABS','OXSQ','SLDB','HPR','ETC','NLC','BIO A','GENT','IIP','EMX','CCFG','FG','MAXR','WCRT','ADH','ACI','TMWE','BIO B','ATNM','TRBR','GKK','PNK','NNVC','CHINA','MHA','KLDX','MFW','BBND','QES','RDI B','GCTS','CNY','FEI','SCPH','HCM','HEV'";
                    //string strCustom = "'MGL','ECMR','CBRE','IDP','ICP','NORT','TRY','PAGS','HLTH','SWTX','UUUU','DGW','NAQ','STST','SRG','EXDX','ARMO','NSE','PVSA','ZOOM','ZNG','FNKO','INTR','WSPT','PSRT','NAK','WWAVB','TLVT','GEDU','DLC'";
                    //string strCustom = "'ESCT','HTI','LSTZB','YPI','NKC','CTEK','CSFS','MPMH','AVM','FLNT','AMAC','USAS','RBSC','FTNW','CDCS','PMI','ORCH','ANDS','NEPG','TORC','NTIP','LMCK','IASO','OLY','LUCA','EAG','PEER'";
                    //string strCustom = "'VEAC'";
                    //string query = string.Format("SELECT  strSymbol FROM tbl_Securities where idStockExchange in ({0})  and strSymbol in ({1}) order by strSymbol", string.Join(", ", exchangeID), strCustom); //'ATNY'     // and strSymbol > 'VMO' 


                    // ************  OR  ******************
                    string query;
                    if (isNewSecurities)
                        // Newly imported daily securities - NEW securities
                        query = string.Format("SELECT  strSymbol FROM tbl_Securities where idStockExchange in ({0}) and importHistorical = 1 order by strSymbol", string.Join(", ", exchangeID));
                    else
                        // ALL securities in given exchanges - RUN ONCE FOR NEW DB
                        query = string.Format("SELECT  strSymbol FROM tbl_Securities where idStockExchange in ({0}) order by strSymbol", string.Join(", ", exchangeID));
                    ////query = string.Format("SELECT  strSymbol FROM tbl_Securities where idStockExchange in ({0}) and  strSymbol in ('HAML') order by strSymbol", string.Join(", ", exchangeID));

                    // ************************************


                    DataTable dtSecurities = getDBTable(query, conn);
                    for (int k = 0; k < dtSecurities.Rows.Count; k++)
                    {
                        secWithDividOrSplit.Add(dtSecurities.Rows[k]["strSymbol"].ToString());
                    }

                    // Exit if there is no securities
                    if (secWithDividOrSplit.Count < 1)
                        return;
                }
                else
                {
                    isDividentsSplits = true;

                    // Select only those symbols that are in tbl_Securities (because array 'secWithDividOrSplit' was build based on Intrinio daily prices. we have to make sure we have those sec in our DB)
                    string query;
                    query = string.Format("SELECT  strSymbol FROM tbl_Securities where strSymbol in ('{0}') order by strSymbol", string.Join("','", secWithDividOrSplit));

                    DataTable dtSecurities = getDBTable(query, conn);
                    secWithDividOrSplit.Clear();
                    for (int k = 0; k < dtSecurities.Rows.Count; k++)
                    {

                        if (!secWithDividOrSplit.Contains(dtSecurities.Rows[k]["strSymbol"].ToString()))
                            secWithDividOrSplit.Add(dtSecurities.Rows[k]["strSymbol"].ToString());
                    }

                    // Exit if there is no securities
                    if (secWithDividOrSplit.Count < 1)
                        return;
                }

                // Loop through all selected securities
                for (int s = 0; s < secWithDividOrSplit.Count; s++)
                {
                    Thread.Sleep(1200);

                    //sec_symbol = dtSecurities.Rows[s]["strSymbol"].ToString();
                    sec_symbol = secWithDividOrSplit[s];
                    try
                    {
                        // Historical prices
                        fullUrl = string.Format(intriMediaURL, sec_symbol);
                        filename = "INTRINIO_Hist_Prices.txt";
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

                        ////logger.WriteLog(Constants.Success, filename, DateTime.MinValue, null, "Succesfull Intrinio historical prices FTP file import for " + sec_symbol, 1, null);
                    }
                    catch (Exception ex)
                    {
                        // There are more than 600 securities (USA) with importHistorical = 1. Every day we try to import historical prices for them.
                        // Most of the time there are no data and download happens too fast and there is error received:
                        // "The remote server returned an error: (429) Too Many Requests."
                        // Do not be alarmed by this, just ignore it.
                        // Not commenting writing to log here in case another error will be received which has to be taken care of.
                        logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, null, "Unsuccesfull Intrinio historical prices FTP file import for " + sec_symbol, 0, ex);
                        continue; // skip to next security
                    }

                    // New table for each security
                    DataTable importedSecuritiesTable = getPricesTableStructure();
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
                                if (Convert.ToDateTime(importedSecuruty["date"].ToString()) >= Convert.ToDateTime("2012-01-01"))
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
                        logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, sp, "Unsuccesfull Intrinio prices table creation", 0, ex);
                        continue;
                    }

                    // DELETE prices in given price range, prior to writing new ones
                    try
                    {
                        sp = Constants.importCleanDataPriorPricesImport;
                        SqlCommand cleanDataSP = new SqlCommand(string.Empty, conn);
                        cleanDataSP.CommandType = System.Data.CommandType.StoredProcedure;
                        cleanDataSP.CommandText = sp;
                        cleanDataSP.CommandTimeout = Constants.SPsTimeout;
                        cleanDataSP.Parameters.Add("@secSymbols_list", SqlDbType.Text).Value = sec_symbol;
                        cleanDataSP.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = string.Join(", ", exchangeID);
                        cleanDataSP.Parameters.Add("@dates_seq", SqlDbType.VarChar, 20).Value = "ALL";
                        //cleanDataSP.Parameters.Add("@start_date", SqlDbType.DateTime).Value = startDate;
                        //cleanDataSP.Parameters.Add("@end_date", SqlDbType.DateTime).Value = endDate;
                        cleanDataSP.ExecuteNonQuery();
                        ////logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, string.Format("Succesfull prices table cleaning for {0}", sec_symbol), 0, null);

                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp,  string.Format("Unsuccessfull prices table cleaning for {0}", sec_symbol), 0, ex);
                        continue;
                    }

                    // Import new historical prices for one security
                    try
                    {
                        sp = Constants.importIntrinioPrices;    
                        SqlCommand securityImportCommand = new SqlCommand(String.Empty, conn);
                        securityImportCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        securityImportCommand.CommandText = sp;
                        securityImportCommand.CommandTimeout = Constants.SPsTimeout;
                        securityImportCommand.Parameters.Add("@imported_prices", SqlDbType.Structured).Value = importedSecuritiesTable;
                        securityImportCommand.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = string.Join(", ", exchangeID);    // id_exchange; //TODO: CHANGE LATER when know what to do with symbols in 2 exchanges
                                                                                                                                             //securityImportCommand.Parameters.Add("@symbol", SqlDbType.VarChar).Value = sec_symbol;  // importedSecuritiesTable.Rows[0]["symbol"];
                                                                                                                                             //securityImportCommand.Parameters.Add("@id_security", SqlDbType.VarChar).Value = id_Security;    // importedSecuritiesTable.Rows[0]["symbol"];
                                                                                                                                             //securityImportCommand.Parameters.Add("@start_date", SqlDbType.DateTime).Value = Convert.ToDateTime(importedSecuritiesTable.Rows[0]["date"]);
                                                                                                                                             //securityImportCommand.Parameters.Add("@end_date", SqlDbType.DateTime).Value = Convert.ToDateTime(importedSecuritiesTable.Rows[importedSecuritiesTable.Rows.Count - 1]["date"]);
                        securityImportCommand.Parameters.Add("@is_historical", SqlDbType.Int).Value = 1;  
                        securityImportCommand.ExecuteNonQuery();

                        string sourceFileFullName = clientFolder + "\\" + filename;
                        string processedFileFullName = processedFolder + "\\" + filename;

                        if (doDeleteFiles == "0")
                            File.WriteAllText(processedFileFullName, fileContent);

                        File.Delete(sourceFileFullName);

                        logger.WriteLog(Constants.Success, filename, DateTime.MinValue, sp, "Succesfull Intrinio historical prices import - " + sec_symbol, 1, null);

                        if (isNewSecurities)
                        {
                            //TODO: update securities table to set importHistorical = 0
                            GeneralFunctions.SetNewSecuritiesToNotNew(conn, string.Format("UPDATE tbl_Securities SET importHistorical = 0 WHERE strSymbol = '{0}'", sec_symbol));                 //LR    Q1: do we have to update it if not successfull ?? 
                            isPricesForNewSecurities = true;    // There are actually prices for at least one new security
                            secImportHistorical.Add(sec_symbol);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, filename, DateTime.MinValue, sp, "Unsuccesfull Intrinio historical prices import - " + sec_symbol, 0, ex);
                        continue;
                    }

                }// For s ... for next security




                if (secWithDividOrSplit.Count > 0 && (isDividentsSplits || (isNewSecurities && isPricesForNewSecurities)))
                {
                    //********************************************************************
                    // Get securities - CUSTOM code
                    //***string strCustom = "'BHO','BEXP','LSTZA','MNLO','APLS','FRZ','PPDI','DHC','GEC','IES','BCON','PPDF','NSI','CLNC','PSAV','WELL','SVI','USFP','HOL','GFLY'";
                    //***string strCustom = "'TMX','ARS','GFIS','AIRI','PRISB','STBC','CLFC','GNTY','SNS','PLC','VIR','GTES','WCNX','DESP','PMAR','LGZ','DCPH','LAC','FSN'";
                    //***string strCustom = "'BPMX','ADSI','ADIL','MOG A','PRS','WHSM','BNHNA','YTI','ANK','DSG','MTLp','CRW','EYPT','BTG','PCC','BFF','ABP','ECP','HUD','SYMS','PNL','DPTR','PMIC','AVST'";
                    //***string strCustom = "'PNL','HJLI','BAS','TTP','ABVC','EEE','GPRC','SWCH','DMI','BQI','JOYG','REI','FNBG','CEU','LRP','ENSV','BB','AUG','MLSS','MBP','ASXWI','ABK','IOP','KWIC','BCBPR','PRSP','AQUA','AIX','MCB','DHCP','FMSPR'";
                    //***string strCustom = "'NINE','ILPT','NKRC','AIP','MF','JBX','PXT','AGS','CAAP','UAGR','LLGX','NTI','CK','NASB','FNC','VMET','SCHK','ARSD','SMTS','DOR','TPHS','MTWS','VICI','ANTE','MBR','DOR','ADLR','EHR','AKC','CURO'";
                    //***string strCustom = "'LMCA','FFH','LCRT','SWB','CAHS','ENP','TOH','CRHM','MOTA','PAET','HAF','INXI','GHQ','FHC','SOK','AVI','PMG','FIZ','MMA','VCC','TPGH','UTA','FTSI','DROOY','FNDT','GECX','WRP','BARI','CIZ','NXG','VSEA'";
                    //***string strCustom = "'FFH','CANF','CASA','ABS','OXSQ','SLDB','HPR','ETC','NLC','BIO A','GENT','IIP','EMX','CCFG','FG','MAXR','WCRT','ADH','ACI','TMWE','BIO B','ATNM','TRBR','GKK','PNK','NNVC','CHINA','MHA','KLDX','MFW','BBND','QES','RDI B','GCTS','CNY','FEI','SCPH','HCM','HEV'";
                    //***string strCustom = "'MGL','ECMR','CBRE','IDP','ICP','NORT','TRY','PAGS','HLTH','SWTX','UUUU','DGW','NAQ','STST','SRG','EXDX','ARMO','NSE','PVSA','ZOOM','ZNG','FNKO','INTR','WSPT','PSRT','NAK','WWAVB','TLVT','GEDU','DLC'";
                    //***string strCustom = "'ESCT','HTI','LSTZB','YPI','NKC','CTEK','CSFS','MPMH','AVM','FLNT','AMAC','USAS','RBSC','FTNW','CDCS','PMI','ORCH','ANDS','NEPG','TORC','NTIP','LMCK','IASO','OLY','LUCA','EAG','PEER'";

                    //***string query = string.Format("SELECT idSecurity FROM tbl_Securities where idStockExchange in ({0})  and strSymbol in ({1}) order by strSymbol", string.Join(", ", exchangeID), strCustom);
                    //********************************************************************

                    if (isNewSecurities && isPricesForNewSecurities)
                    {
                        secWithDividOrSplit.Clear();

                        // Select securities with newly imported historical data
                        for (int w = 0; w < secImportHistorical.Count; w++)
                        {
                            secWithDividOrSplit.Add(secImportHistorical[w]);
                        }
                    }

                    //***********  OR ***********
                    ////secWithDividOrSplit = secImportHistorical;

                    // Get securities
                    string security_list = "";
                    string query = string.Format("SELECT idSecurity FROM tbl_Securities where idStockExchange in ({0}) and strSymbol in ('{1}') order by idSecurity", string.Join(", ", exchangeID), string.Join("','", secWithDividOrSplit));

                    DataTable dtSecurities = getDBTable(query, conn);
                    for (int k = 0; k < dtSecurities.Rows.Count; k++)
                    {
                        security_list += string.Format(",{0}", dtSecurities.Rows[k]["idSecurity"].ToString());
                    }
                    if (security_list.Length > 0)
                        security_list = security_list.Substring(1);

                    // In case of importing new historical prices for securities with DIVIDENTS OR SPLITS,
                    // we need to run 4 SPs for THESE securities only (PASS  Security list!!!):
                    //  * dataCompleteMissingPrices
                    //  * dataDuplicateLastDayPrices
                    //  * dataSetIsTraded
                    //  * calcPriceReturns


                    //  ***** dataCompleteMissingPrices *****
                    try
                    {
                        sp = Constants.CompleteMissingPrices;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        command.Parameters.Add("@dates_seq", SqlDbType.VarChar).Value = "ALL"; // !!!WARNING: ONCE HAVE TO RUN WITH 'ALL'
                        command.Parameters.Add("@start_date", SqlDbType.DateTime).Value = DateTime.Today;    // not used when 'ALL'
                        command.Parameters.Add("@end_date", SqlDbType.DateTime).Value = DateTime.Today;      // not used when 'ALL'
                        command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = string.Join(", ", exchangeID);
                        command.Parameters.Add("@security_list", SqlDbType.VarChar).Value = security_list;
                        command.ExecuteNonQuery();
                        completeMissingPricesFillingSuccess = true;
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull CompleteMissingPrices historical refilling for " + security_list, 0, null);
                    }
                    catch (Exception ex)
                    {
                        completeMissingPricesFillingSuccess = false;
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull CompleteMissingPrices historical refilling for " + security_list, 0, ex);
                    }

                    //  ****** dataDuplicateLastDayPrices ******
                    try
                    {
                        sp = Constants.DuplicateLastDayPrices;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        command.Parameters.Add("@start_date", SqlDbType.DateTime).Value = DateTime.Today.AddDays(-4);
                        command.Parameters.Add("@end_date", SqlDbType.DateTime).Value = DateTime.Today;
                        command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = string.Join(", ", exchangeID);
                        command.Parameters.Add("@security_list", SqlDbType.VarChar).Value = security_list;
                        command.ExecuteNonQuery();
                        duplicateLastDayPricesSuccess = true;
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull DuplicateLastDayPrices filling for " + security_list, 0, null);
                    }
                    catch (Exception ex)
                    {
                        duplicateLastDayPricesSuccess = false;
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull DuplicateLastDayPrices filling for " + security_list, 0, ex);
                    }

                    //  ****** dataSetIsTraded ******
                    try
                    {
                        sp = Constants.SetIsTraded;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = string.Join(", ", exchangeID);
                        command.Parameters.Add("@security_list", SqlDbType.VarChar).Value = security_list;
                        command.ExecuteNonQuery();
                        setIsTradedSuccess = true;
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull non-trading securities detecting for " + security_list, 0, null);
                    }
                    catch (Exception ex)
                    {
                        setIsTradedSuccess = false;
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull non-trading securities detecting for " + security_list, 0, ex);
                    }

                    //  ****** dataSetDuplicateSecToNonTraded ******
                    try
                    {
                        sp = Constants.SetDuplicateSymbols;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        //If adding exchangesString parameter, should call SP twice: 1. '3,4,5'    2. '1'
                        //command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = exchangesString;
                        command.ExecuteNonQuery();
                        setIsTradedSuccess = true;
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull duplicate symbols securities detecting", 0, null);
                    }
                    catch (Exception ex)
                    {
                        setIsTradedSuccess = false;
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull duplicate symbols securities detecting", 0, ex);
                    }









                    if (setIsTradedSuccess && completeMissingPricesFillingSuccess && duplicateLastDayPricesSuccess)
                    {
                        //  ****** calcPriceReturns ******
                        try
                        {
                            sp = Constants.PriceReturns;
                            SqlCommand command = new SqlCommand(string.Empty, conn);
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.CommandText = sp;
                            command.CommandTimeout = Constants.SPsTimeout;
                            command.Parameters.Add("@is_initial_build", SqlDbType.Int).Value = 1;
                            command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = string.Join(", ", exchangeID);
                            command.Parameters.Add("@is_index", SqlDbType.Int).Value = 0;   // from tbl_Securities
                            command.Parameters.Add("@security_list", SqlDbType.VarChar).Value = security_list;
                            command.ExecuteNonQuery();
                            calcPriceReturnsSuccess = true;
                            logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull securities price returns calculation for " + security_list, 0, null);
                        }
                        catch (Exception ex)
                        {
                            calcPriceReturnsSuccess = false;
                            logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull securities price returns calculation for " + security_list, 0, ex);
                        }
                    }

                }// IF(isDividentsSplits || isNewSecurities)...

                //logger.WriteLog(Constants.Success, filename, DateTime.MinValue, sp, "Succesfull Intrinio historical prices import ALL", 1, null);

            }// using
        }

        private static bool getPriceRow(DataRow importedSecuruty, string securityRec, string sec_symbol, double shekToAgorot)
        {
            try
            {
                string[] security = securityRec.Split(new char[] { ',' });

                importedSecuruty["symbol"] = sec_symbol;
                importedSecuruty["date"] = security[0];
                importedSecuruty["open"] = Convert.ToDouble(security[1]) * shekToAgorot;
                importedSecuruty["close"] = Convert.ToDouble(security[4]) * shekToAgorot;
                importedSecuruty["volume"] = security[5].Trim();    
                importedSecuruty["dividend"] = security[6];
                importedSecuruty["split"] = security[7];
                importedSecuruty["adjOpen"] = Convert.ToDouble(security[8]) * shekToAgorot; 
                importedSecuruty["adjClose"] = Convert.ToDouble(security[11]) * shekToAgorot;
                importedSecuruty["adjVolume"] = security[12];
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

    }
}
