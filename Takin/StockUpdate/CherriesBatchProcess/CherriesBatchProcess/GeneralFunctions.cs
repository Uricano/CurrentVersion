using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Net.Http;
using System.Configuration;

namespace CherriesBatchProcess
{
    public static class GeneralFunctions

    {

        public static void SetLastUpdateDate(SqlConnection conn, int idStockExch)
        {   // SET field 'LastUpdate' in the table 'tblSel_StockExchanges' to TODAY's date

            try
            {
                SqlCommand importSecsSP = new SqlCommand(string.Empty, conn);
                importSecsSP.CommandType = System.Data.CommandType.Text;
                importSecsSP.CommandText = string.Format("UPDATE tblSel_StockExchanges SET LastUpdate = CAST(Getdate() as datetime) WHERE idStockExchange = {0}", idStockExch);
                importSecsSP.CommandTimeout = Constants.SPsTimeout;
                importSecsSP.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
        }

        public static void NotifyCherriesApp()
        {
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(2, 0, 0);
            var url = ConfigurationManager.AppSettings["CherriesUrl"] + "api/Optimization";
            var res = client.GetAsync(url).Result;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[Constants.DB_ConnectionString_Quandl].ConnectionString))
            {
                conn.Open();
                Logger logger = new Logger(conn, Constants.UpdateCherries);
                if (res.IsSuccessStatusCode)
                    logger.WriteLog(Constants.Success, Constants.UpdateCherries, DateTime.MinValue, "", "Succesfull update cherries - Number of Securites: " + res.Content.ReadAsStringAsync().Result, 0, null);
                else
                    logger.WriteLog(Constants.Failure, Constants.UpdateCherries, DateTime.MinValue, "", "Unsuccesfull update cherries " + res.Content.ReadAsStringAsync().Result, 0, null);
            }
        }

		public static void SetLastUpdateDate(SqlConnection conn, string strSymbol)
        {   // SET field 'LastUpdate' in the table 'tblSel_StockExchanges' to TODAY's date

            try
            {
                SqlCommand importSecsSP = new SqlCommand(string.Empty, conn);
                importSecsSP.CommandType = System.Data.CommandType.Text;
                importSecsSP.CommandText = string.Format("UPDATE tblSel_StockExchanges SET LastUpdate = CAST(Getdate() as datetime) WHERE strSymbol = '{0}'", strSymbol);
                importSecsSP.CommandTimeout = Constants.SPsTimeout;
                importSecsSP.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
        }

        public static void SetNewSecuritiesToNotNew(SqlConnection conn, string strSQL)
        {   // SET field 'importHistorical' in the table 'tbl_Securities' to 0

            try
            {
                SqlCommand importSecsSP = new SqlCommand(string.Empty, conn);
                importSecsSP.CommandType = System.Data.CommandType.Text;
                importSecsSP.CommandText = strSQL;
                importSecsSP.CommandTimeout = Constants.SPsTimeout;
                importSecsSP.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
        }


    }
}
