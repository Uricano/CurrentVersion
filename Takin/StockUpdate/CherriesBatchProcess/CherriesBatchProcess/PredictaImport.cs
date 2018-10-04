/**
 * args:
 * interval_for_applause_zip_choosing [-s] [-p] [-i]
 * 
 * -s securities import
 * -p prices import
 * -i indexes rates import
 *  
 * interval_for_applause_zip_choosing   
 * 2014/02/26 2017/02/27       between dates
 * -daily                      files by today and yesterday
 * -last                       last accessible file by date in filename
 * For securities import the file always is the last from specified interval - we hope it contains all securities including ones new.
 * For indexes rates import the file always is the last from specified interval - it contains rates for all dates.
 * For prices import the files contain only last data thus we take all files from interval.
 * 
 * For securities we insert non-existing in the table and update others.
 * For prices we import data in specified interval with overwriting of existing, for indices we fill all last dates missing in the table. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Net;
using System.Data;
using System.Data.SqlClient;

using Ionic.Zip;

namespace CherriesBatchProcess
{
    public class PredictaImport
    {
        public static void Execute(string[] args, Dictionary<string, string> config)
        {
            string clientFolder = null;
            string processedFolder = null;
            string tempFolder = null;
            //string subTempFolder = null;
            string rejectedFolder = null;
            string ftpServerUrl = null;
            string ftpUserName = null;
            string ftpPassword = null;
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MaxValue;
            bool processSecurities = false;
            bool processPrices = false;
            bool processIndexes = false;
            string doDeleteFiles = null;

            clientFolder = config[Constants.SourceDir];
            processedFolder = config[Constants.ProcessedDir];
            rejectedFolder = config[Constants.RejectedDir];
            tempFolder = config[Constants.TempDir];
            ftpServerUrl = config[Constants.PredictaFTPServerURL];
            ftpUserName = config[Constants.PredictaFTPUserName];
            ftpPassword = config[Constants.PredictaFTPPassword];
            doDeleteFiles = config[Constants.DoDeleteFiles];

            using (SqlConnection conn = new SqlConnection(config[Constants.DB_ConnectionString]))
            {
                conn.Open();
                Logger logger = null;

                try
                {
                    if (args[0].ToLower() == Constants.Daily)
                    {
                        logger = new Logger(conn, Constants.PredictaImport);
                        startDate = DateTime.Today.AddDays(-1);
                        endDate = DateTime.Today;
                    }
                    else //LR - added 'ELSE'
                        if (args[0].ToLower() == Constants.Last)
                        {
                            logger = new Logger(conn, Constants.PredictaImport);
                        }
                        else
                        {
                            logger = new Logger(conn, Constants.PredictaHistImport);
                            startDate = DateTime.ParseExact(args[0], Constants.DateParmsFormat, System.Globalization.CultureInfo.CurrentCulture);
                            endDate = DateTime.ParseExact(args[1], Constants.DateParmsFormat, System.Globalization.CultureInfo.CurrentCulture);
                        }

                    foreach (string arg in args)
                    {
                        if (arg.ToLower() == Constants.Securities) processSecurities = true;
                        if (arg.ToLower() == Constants.Prices) processPrices = true;
                        if (arg.ToLower() == Constants.Indeces) processIndexes = true;
                    }
                    if (processSecurities == false && processPrices == false && processIndexes == false) throw new Exception("Incorrect parameters");
                }
                catch (Exception ex)
                {
                    logger.WriteLog(Constants.Failure, null, DateTime.MinValue, null, "Accept command-line parameters", 0, ex);
                    return;
                }

                // Import FTP files
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

                List<string> selectedFiles = new List<string>();

                if (args[0].ToLower() == Constants.Last)
                {
                    DateTime lastDate = DateTime.MinValue;
                    foreach (string filename in files)
                    {
                        if (GetDateByFilename(filename) > lastDate) lastDate = GetDateByFilename(filename);
                    }

                    if (lastDate == DateTime.MinValue) return;

                    startDate = lastDate.AddDays(-1);
                    endDate = lastDate;
                }

                foreach (string filename in files)
                {
                    if ((GetDateByFilename(filename) >= startDate) && (GetDateByFilename(filename) <= endDate)) selectedFiles.Add(filename);
                }

                PredictaFilenamesComparer filenamesComparer = new PredictaFilenamesComparer();
                selectedFiles.Sort(filenamesComparer);

                int i = -1;
                //int fileCount = 0;
                foreach (string filename in selectedFiles)
                {
                    //fileCount++;
                    //subTempFolder = tempFolder + "\\subFolder" + fileCount;

                    i++;
                    string fullFilename = null;

                    try
                    {
                        string fullUrl = ftpServerUrl + "/" + filename;
                        fullFilename = clientFolder + "\\" + filename;
                        FtpWebRequest requestf = (FtpWebRequest)WebRequest.Create(fullUrl);
                        requestf.KeepAlive = false;
                        requestf.Method = WebRequestMethods.Ftp.DownloadFile;
                        requestf.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                        FtpWebResponse respf = (FtpWebResponse)requestf.GetResponse();
                        Stream respfStream = respf.GetResponseStream();
                        if (!Directory.Exists(clientFolder)) Directory.CreateDirectory(clientFolder);
                        CopyFile(respfStream, null, fullFilename);
                        respfStream.Close();
                        respf.Close();

                        logger.WriteLog(Constants.Success, filename, GetDateByFilename(filename), null, "Succesfull FTP zip file import", 1, null);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Error, filename, GetDateByFilename(filename), null, "Unsuccessfull FTP zip file import", 0, ex);
                        continue;
                    }


                    // Unzip FTP files
                    // TODO: Verify zip files are deleted after usage
                    try
                    {
                        //if (Directory.Exists(tempFolder))
                        //{
                        //    Directory.Delete(tempFolder, true);
                        //}
                        //Directory.CreateDirectory(tempFolder);

                        // Reset temporary folder - clear files (by LR)
                        if (Directory.Exists(tempFolder))
                        {
                            //Directory.Delete(tempFolder, true);
                            string[] previousFiles = Directory.GetFiles(tempFolder);
                            for (int jj = 0; jj < previousFiles.Length; jj++)
                            {
                                File.Delete(previousFiles[jj]);
                            }
                        }
                        else
                            Directory.CreateDirectory(tempFolder);

                        using (ZipFile zip = ZipFile.Read(fullFilename))
                        {
                            zip.FlattenFoldersOnExtract = true;
                            zip.ExtractAll(tempFolder);
                        }

                        logger.WriteLog(Constants.Success, filename, GetDateByFilename(filename), null, "Succesfull unzip", 1, null);
                    }
                    catch (Exception ex)
                    {
                        if (doDeleteFiles == "0")
                           CopyFile(null, fullFilename, rejectedFolder + "\\" + filename);

                        File.Delete(fullFilename);
                        logger.WriteLog(Constants.Error, filename, GetDateByFilename(filename), null, "Unsuccesfull unzip", 0, ex);
                        continue;
                    }

                    bool successFileProcessing = true;


                    // Import Securities from XML file
                    if (processSecurities)
                    {
                        string sp = Constants.importPredictaSecuritiesImport;
                        try
                        {
                            StreamReader f = new StreamReader(tempFolder + "\\" + Constants.PredictaSecuritiesFilename);
                            SqlCommand importSecsSP = new SqlCommand(string.Empty, conn);
                            importSecsSP.CommandType = System.Data.CommandType.StoredProcedure;
                            importSecsSP.CommandText = sp;
                            importSecsSP.CommandTimeout = Constants.SPsTimeout;
                            importSecsSP.Parameters.Add("@xml_file", SqlDbType.Xml).Value = f.ReadToEnd();
                            f.Close();
                            //importSecsSP.Parameters.Add("@exchange", SqlDbType.VarChar, 20).Value = exchange;
                            importSecsSP.ExecuteNonQuery();

                            logger.WriteLog(Constants.Success, filename, GetDateByFilename(filename), sp, "Succesfull securities import", 1, null);
                        }
                        catch (Exception ex)
                        {
                            successFileProcessing = false;
                            if (doDeleteFiles == "0")
                                CopyFile(null, fullFilename, rejectedFolder + "\\" + filename);

                            File.Delete(fullFilename);
                            logger.WriteLog(Constants.Error, filename, GetDateByFilename(filename), sp, "Unsuccessfull securities import", 0, ex);
                        }
                    }


                    // Import Prices from XML file
                    if (processPrices)
                    {
                        string sp = Constants.importPredictaPricesImport;
                        try
                        {
                            SqlCommand importPricesSP = new SqlCommand(string.Empty, conn);
                            importPricesSP.CommandType = System.Data.CommandType.StoredProcedure;
                            importPricesSP.CommandText = sp;
                            importPricesSP.CommandTimeout = Constants.SPsTimeout;
                            StreamReader f = new StreamReader(tempFolder + "\\" + Constants.PredictaPricesFilename);
                            importPricesSP.Parameters.Add("@xml_file", SqlDbType.Xml).Value = f.ReadToEnd();
                            f.Close();
                            importPricesSP.Parameters.Add("@start_date", SqlDbType.DateTime).Value = startDate;
                            importPricesSP.Parameters.Add("@end_date", SqlDbType.DateTime).Value = endDate;
                            importPricesSP.ExecuteNonQuery();

                            logger.WriteLog(Constants.Success, filename, GetDateByFilename(filename), sp, "Succesfull prices import", 1, null);
                        }
                        catch (Exception ex)
                        {
                            successFileProcessing = false;
                            if (doDeleteFiles == "0")
                               CopyFile(null, fullFilename, rejectedFolder + "\\" + filename);

                            File.Delete(fullFilename);
                            logger.WriteLog(Constants.Error, filename, GetDateByFilename(filename), sp, "Unsuccessfull prices import", 0, ex);
                        }
                    }

                    // Import Indices from XML file
                    if (processIndexes)
                    {
                        string sp = Constants.importPredictaIndexRatesImport;
                        try
                        {
                            SqlCommand importIndRatesSP = new SqlCommand(string.Empty, conn);
                            importIndRatesSP.CommandType = System.Data.CommandType.StoredProcedure;
                            importIndRatesSP.CommandText = sp;
                            importIndRatesSP.CommandTimeout = Constants.SPsTimeout;
                            StreamReader f = new StreamReader(tempFolder + "\\" + Constants.PredictaIndecesFilename);
                            importIndRatesSP.Parameters.Add("@xml_file", SqlDbType.Xml).Value = f.ReadToEnd();
                            f.Close();
                            importIndRatesSP.Parameters.Add("@start_date", SqlDbType.DateTime).Value = startDate;
                            importIndRatesSP.Parameters.Add("@end_date", SqlDbType.DateTime).Value = endDate;
                            importIndRatesSP.Parameters.Add("@i", SqlDbType.Int).Value = i;
                            importIndRatesSP.ExecuteNonQuery();

                            logger.WriteLog(Constants.Success, filename, GetDateByFilename(filename), sp, "Succesfull indexes rates import", 1, null);
                        }
                        catch (Exception ex)
                        {
                            successFileProcessing = false;
                            if (doDeleteFiles == "0")
                                CopyFile(null, fullFilename, rejectedFolder + "\\" + filename);

                            File.Delete(fullFilename);
                            logger.WriteLog(Constants.Error, filename, GetDateByFilename(filename), sp, "Unsuccessfull indexes rates import", 0, ex);
                        }
                    }

                    // TODO: Update tblSel_StockExchanges.LastUpdate for proper Exchange for Today
                    GeneralFunctions.SetLastUpdateDate(conn, Constants.PredictaExchangeId);    //LR    Q1: do we have to update it if not successfull ?? 

                    if (successFileProcessing)
                    {
                        if (doDeleteFiles == "0")
                            CopyFile(null, fullFilename, processedFolder + "\\" + filename);

                        File.Delete(fullFilename);
                    } //LR In case of unsuccessful there is delete in "CATCH" clause of each "TRY" block
                }
            }
        }

        public static DateTime GetDateByFilename(string filename)
        {
            if (filename.Length >= Constants.PredictaFilenameLength && filename.Substring(0, Constants.PredictaFilenamePrefix.Length) == Constants.PredictaFilenamePrefix)
            {
                return DateTime.ParseExact(
                    filename.Substring(Constants.PredictaFilenamePrefix.Length, Constants.PredictaDateInFilenameFormat.Length), 
                    Constants.PredictaDateInFilenameFormat, System.Globalization.CultureInfo.CurrentCulture);
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        private static void CopyFile (Stream istream, string ifilename, string ofilename)
        {
            Stream istream_local = ((istream == null) ? new FileStream(ifilename, FileMode.Open) : istream);
            FileStream ostream = File.Create(ofilename);

            byte[] buffer = new byte[32 * 1024];
            int read;

            while ((read = istream_local.Read(buffer, 0, buffer.Length)) > 0)
            {
                ostream.Write(buffer, 0, read);
            }

            ostream.Close();
            if (istream == null) istream_local.Close(); 
        }

        public class PredictaFilenamesComparer : IComparer<string>
        {
            public int Compare(string filename1, string filename2)
            {
                return GetDateByFilename(filename1).CompareTo(GetDateByFilename(filename2));
            }
        }
    }
}
