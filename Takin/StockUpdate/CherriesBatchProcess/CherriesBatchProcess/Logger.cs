using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace CherriesBatchProcess
{
    public class Logger
    {
        private SqlConnection conn = null;
        private string process = null;
        private string processId = null;

        private string[,] processes = 
        {
            {Constants.EODSecuritiesImport, Constants.EODSecuritiesImportProcessId},
            {Constants.EODPricesImport, Constants.EODPricesImportProcessId},
            {Constants.PredictaImport, Constants.PredictaImportProcessId},
            {Constants.EODHistPricesImport, Constants.EODHistPricesImportProcessId},
            {Constants.PredictaHistImport, Constants.PredictaHistImportProcessId},
            {Constants.BatchCalculation, Constants.BatchCalculationProcessId},
            {Constants.UpdateCherries, Constants.UpdateCherriesProcessId},
            {Constants.QuandlSecuritiesImport, Constants.QuandlSecuritiesImportProcessId},
            {Constants.QuandlPricesImport, Constants.QuandlPricesImportProcessId},
            {Constants.IntrinioPricesImport, Constants.IntrinioPricesImportProcessId},
            {Constants.IntrinioSecuritiesImport, Constants.IntrinioSecuritiesImportProcessId},
            {Constants.IntrinioBatchCalculation, Constants.IntrinioBatchCalculationProcessId}
       };

        public Logger(SqlConnection conn, string process)
        {
            this.conn = conn;
            this.process = process;
            for (int i = 0; i < processes.GetLength(0); i++)
            {
                if (process == processes[i, 0]) this.processId = processes[i, 1];  
            }
        }

        public void WriteLog(string infoType, string filename, DateTime fileDate, string spName, string actionDescription, int importStatus, Exception ex)
        {
            Console.WriteLine(infoType + "  " + actionDescription + "  " + ((spName == null) ? "" : spName) + "  " + ((filename == null) ? "" : filename) + "  " + ((ex == null) ? "" : ex.Message));

            if (conn != null)
            {
                string dt = ((fileDate == DateTime.MinValue) ? "null" : ("'" + fileDate.ToString("yyyy/MM/dd") + "'")); 
                string now = "'" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.FFF") + "'";
                string filename_to_insert = ((filename == null) ? "null" : ("'" + filename + "'"));
                string sp_to_insert = ((spName == null) ? "null" : ("'" + spName + "'"));

                string logSql =
                "insert into tb_ProcessLog (ProcessId, LogDate, ArchiveFileName, ArchiveFileCreatedDate, ClassName, MethodName, SPName, InfoType, ActionDescription, ImportStatus) " +
                "values (" +
                processId + ", " + now + ", " + filename_to_insert + ", " + dt + ", '" + process + "', null, " + sp_to_insert + ", '" + infoType + "', '" + actionDescription + "', " + importStatus.ToString() + ")";

                try
                {
                    SqlCommand logCommand = new SqlCommand(logSql, conn);
                    logCommand.ExecuteNonQuery();
                }
                catch (Exception writeLogEx)
                {
                    Console.WriteLine("Log table writing exception: " + writeLogEx.Message);
                }

                if (infoType == "F" || infoType == "E")
                {
                    string exstr = ((ex == null) ? "null" : ("'" + ex.Message + "'"));

                    string exSql =
                    "insert into tb_ProcessExceptions (ProcessId, LogDate, ArchiveFileName, ArchiveFileCreatedDate, ClassName, MethodName, SPName, InfoType, ActionDescription, ImportStatus, strException) " +
                    "values (" +
                    processId + ", " + now + ", " + filename_to_insert + ", " + dt + ", '" + process + "', null, " + sp_to_insert + ", '" + infoType + "', '" + actionDescription + "', " + importStatus.ToString() + ", " +
                    exstr + ")";

                    try
                    {
                        SqlCommand exCommand = new SqlCommand(exSql, conn);
                        exCommand.ExecuteNonQuery();
                    }
                    catch (Exception writeExEx)
                    {
                        Console.WriteLine("Exceptions table writing exception: " + writeExEx.Message);
                    }
                }
            }
        }
    }
}
