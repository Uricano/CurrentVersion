using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace CherriesBatchProcess
{
    public class BatchCalculations
    {
        public static void Execute(Dictionary<string, string> config, int[] exchangesIds)
        {
            string connectionString = config[Constants.DB_ConnectionString];
            int chooseSecuritiesForexchange = Int32.Parse(config[Constants.ChooseSecuritiesForExchange]);
            DateTime startDate = DateTime.Today.AddDays(-1).AddYears(-3); ;
            DateTime startDateMissingPrc = DateTime.Today.AddDays(-60);
            DateTime endDate = DateTime.Today.AddDays(-1); ; 

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                Logger logger = new Logger(conn, Constants.BatchCalculation);
                
                bool shekelLeShekelSuccess = false;
                bool setIsTradedSuccess = false;
                bool completeMissingPricesFillingSuccess = false;
                bool completeMissingIndexPricesFillingSuccess = false;
                bool calcPriceReturnsSuccess = false;
                bool calcAvgYieldSuccess = false;
                bool calcStdYieldSuccess = false;
                bool calcMonetaryAvgSuccess = false;
                bool duplicateLastDayPricesSuccess = false;
                bool duplicateLastDayIndexPricesSucces = false;

                ////StringBuilder sbExchangesString = new StringBuilder();
                ////for (int i = 0; i < exchangesIds.Length; i++)
                ////{
                ////    sbExchangesString.Append((i > 0) ? "," : "");
                ////    sbExchangesString.Append(exchangesIds[i].ToString());
                ////}
                ////string exchangesString = sbExchangesString.ToString();

                //    OR

                string exchangesString = string.Join(",", exchangesIds);

                string sp = null;
                try
                {
                    sp = Constants.ShekelLeShekel;
                    SqlCommand command = new SqlCommand(string.Empty, conn);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = sp;
                    command.CommandTimeout = Constants.SPsTimeout;
                    command.ExecuteNonQuery();
                    shekelLeShekelSuccess = true;
                    logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Succesfull shekel course filling", 0, null);
                }
                catch (Exception ex)
                {
                    shekelLeShekelSuccess = false;
                    logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccesfull shekel course filling", 0, ex);
                }

                try
                {
                    sp = Constants.CompleteMissingPrices;
                    SqlCommand command = new SqlCommand(string.Empty, conn);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = sp;
                    command.CommandTimeout = Constants.SPsTimeout;
                    //command.Parameters.Add("@sec_types_list", SqlDbType.VarChar).Value = "ALL";
                    command.Parameters.Add("@dates_seq", SqlDbType.VarChar).Value = "range";
                    command.Parameters.Add("@start_date", SqlDbType.DateTime).Value = startDateMissingPrc;  // endDate.AddDays(-4);
                    command.Parameters.Add("@end_date", SqlDbType.DateTime).Value = endDate;
                    command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = exchangesString;
                    command.ExecuteNonQuery();
                    completeMissingPricesFillingSuccess = true;
                    logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull CompleteMissingPrices filling", 0, null);
                }
                catch (Exception ex)
                {
                    completeMissingPricesFillingSuccess = false;
                    logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull CompleteMissingPrices filling", 0, ex);
                }

                try
                {
                    sp = Constants.CompleteMissingIndexPrices;
                    SqlCommand command = new SqlCommand(string.Empty, conn);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = sp;
                    command.CommandTimeout = Constants.SPsTimeout;
                    command.Parameters.Add("@dates_seq", SqlDbType.VarChar).Value = "LAST";
                    command.Parameters.Add("@start_date", SqlDbType.DateTime).Value = startDateMissingPrc;  // endDate.AddDays(-4); ;
                    command.Parameters.Add("@end_date", SqlDbType.DateTime).Value = endDate;
                    command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = "1,-2"; // LR - added. DO WE NEED IT? Or it just should be 'ALL'???
                    command.ExecuteNonQuery();
                    completeMissingIndexPricesFillingSuccess = true;
                    logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull DuplicateLastDayIndexPrices filling", 0, null);
                }
                catch (Exception ex)
                {
                    completeMissingIndexPricesFillingSuccess = false;
                    logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull DuplicateLastDayIndexPrices filling", 0, ex);
                }

                try
                {
                    sp = Constants.DuplicateLastDayPrices;
                    SqlCommand command = new SqlCommand(string.Empty, conn);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = sp;
                    command.CommandTimeout = Constants.SPsTimeout;
                    //command.Parameters.Add("@sec_types_list", SqlDbType.VarChar).Value = "ALL";
                    //command.Parameters.Add("@dates_seq", SqlDbType.VarChar).Value = "range";
                    command.Parameters.Add("@start_date", SqlDbType.DateTime).Value = DateTime.Today.AddDays(-4);
                    command.Parameters.Add("@end_date", SqlDbType.DateTime).Value = DateTime.Today;
                    command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = exchangesString;
                    command.ExecuteNonQuery();
                    duplicateLastDayPricesSuccess = true;
                    logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull DuplicateLastDayPrices filling", 0, null);
                }
                catch (Exception ex)
                {
                    duplicateLastDayPricesSuccess = false;
                    logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull DuplicateLastDayPrices filling", 0, ex);
                }

                try
                {
                    sp = Constants.DuplicateLastDayIndexPrices;
                    SqlCommand command = new SqlCommand(string.Empty, conn);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = sp;
                    command.CommandTimeout = Constants.SPsTimeout;
                    //command.Parameters.Add("@dates_seq", SqlDbType.VarChar).Value = "range";
                    command.Parameters.Add("@start_date", SqlDbType.DateTime).Value = DateTime.Today.AddDays(-4); ;
                    command.Parameters.Add("@end_date", SqlDbType.DateTime).Value = DateTime.Today;
                    command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = "1,-2"; // LR - added. DO WE NEED IT? Or it just should be 'ALL'???
                    command.ExecuteNonQuery();
                    duplicateLastDayIndexPricesSucces = true;
                    logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull DuplicateLastDayIndexPrices filling", 0, null);
                }
                catch (Exception ex)
                {
                    duplicateLastDayIndexPricesSucces = false;
                    logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull DuplicateLastDayIndexPrices filling", 0, ex);
                }

                try
                {
                    sp = Constants.SetIsTraded;
                    SqlCommand command = new SqlCommand(string.Empty, conn);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = sp;
                    command.CommandTimeout = Constants.SPsTimeout;
                    command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = exchangesString;
                    command.ExecuteNonQuery();
                    setIsTradedSuccess = true;
                    logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull non-trading securities detecting", 0, null);
                }
                catch (Exception ex)
                {
                    setIsTradedSuccess = false;
                    logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull non-trading securities detecting", 0, ex);
                }

                if (shekelLeShekelSuccess && setIsTradedSuccess && completeMissingPricesFillingSuccess && completeMissingIndexPricesFillingSuccess && duplicateLastDayPricesSuccess && duplicateLastDayIndexPricesSucces)
                {
                    // Securities
                    string[] exchs = exchangesString.Split(new char[] { ',' });
                    for (int i = 0; i < exchs.Length; i++)
                    {
                        try
                        {
                            sp = Constants.PriceReturns;
                            SqlCommand command = new SqlCommand(string.Empty, conn);
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.CommandText = sp;
                            command.CommandTimeout = Constants.SPsTimeout;
                            command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = exchs[i];
                            command.Parameters.Add("@is_index", SqlDbType.Int).Value = 0;   // from tb_Securities
                            command.ExecuteNonQuery();
                            calcPriceReturnsSuccess = true;
                            logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull securities weekly price returns calculation. Exchange id " + exchs[i], 0, null);
                        }
                        catch (Exception ex)
                        {
                            calcPriceReturnsSuccess = false;
                            logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull securities weekly price returns calculation. Exchange id " + exchs[i], 0, ex);
                        }
                    }
                    //---------------------------------------
                    // Indices
                    try
                    {
                        sp = Constants.PriceReturns;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = "1,-2";
                        command.Parameters.Add("@is_index", SqlDbType.Int).Value = 1;   // from tbl_Indices
                        command.ExecuteNonQuery();
                        //calcPriceReturnsSuccess = true;
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull indices weekly price returns calculation.", 0, null);
                    }
                    catch (Exception ex)
                    {
                        //calcPriceReturnsSuccess = false;
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull indices weekly price returns calculation.", 0, ex);
                    }

                    //try
                    //{
                    //    sp = Constants.PriceReturns;
                    //    SqlCommand command = new SqlCommand(string.Empty, conn);
                    //    command.CommandType = System.Data.CommandType.StoredProcedure;
                    //    command.CommandText = sp;
                    //    command.CommandTimeout = Constants.SPsTimeout;
                    //    command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = exchangesString;
                    //    command.ExecuteNonQuery();
                    //    calcPriceReturnsSuccess = true;
                    //    logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull weekly price returns calculation", 0, null);
                    //}
                    //catch (Exception ex)
                    //{
                    //    calcPriceReturnsSuccess = false;
                    //    logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull weekly price returns calculation", 0, ex);
                    //}

                    //======================================================================================
                    // WEEKLY price returns calculations - for BACKTESTING

                    // Securities
                    ////string[] exchs = exchangesString.Split(new char[] { ',' });
                    for (int i = 0; i < exchs.Length; i++)
                    {
                        try
                        {
                            sp = Constants.PriceReturnsWeekly;
                            SqlCommand command = new SqlCommand(string.Empty, conn);
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.CommandText = sp;
                            command.CommandTimeout = Constants.SPsTimeout;
                            command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = exchs[i];
                            command.Parameters.Add("@is_index", SqlDbType.Int).Value = 0;   // from tb_Securities
                            command.ExecuteNonQuery();
                            ///////calcPriceReturnsSuccess = true; // DO we need some kind of flag here?
                            logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull securities weekly price returns calculation (backtesting). Exchange id " + exchs[i], 0, null);
                        }
                        catch (Exception ex)
                        {
                            calcPriceReturnsSuccess = false;
                            logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull securities weekly price returns calculation (backtesting). Exchange id " + exchs[i], 0, ex);
                        }
                    }
                    //---------------------------------------
                    // Indices
                    try
                    {
                        sp = Constants.PriceReturnsWeekly;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = "1,-2";
                        command.Parameters.Add("@is_index", SqlDbType.Int).Value = 1;   // from tbl_Indices
                        command.ExecuteNonQuery();
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull indices weekly price returns calculation (backtesting).", 0, null);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull indices weekly price returns calculation (backtesting).", 0, ex);
                    }

                }

                if (calcPriceReturnsSuccess)
                {
                    // ********************************   AvgYield  ****************************************
                    // Securities
                    try
                    {
                        sp = Constants.AvgYield;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = exchangesString;
                        command.Parameters.Add("@is_index", SqlDbType.Int).Value = 0;   // from tb_Securities
                        command.ExecuteNonQuery();
                        calcAvgYieldSuccess = true;
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull securities average yields calculation", 0, null);
                    }
                    catch (Exception ex)
                    {
                        calcAvgYieldSuccess = false;
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull securities average yields calculation", 0, ex);
                    }
                    //---------------------------------------
                    // Indices
                    try
                    {
                        sp = Constants.AvgYield;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = "1,-2";
                        command.Parameters.Add("@is_index", SqlDbType.Int).Value = 1;   // from tbl_Indices
                        command.ExecuteNonQuery();
                        //calcAvgYieldSuccess = true;
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull indices average yields calculation", 0, null);
                    }
                    catch (Exception ex)
                    {
                        //calcAvgYieldSuccess = false;
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull indices average yields calculation", 0, ex);
                    }
                    // ****************************************************************************************

                    // **********************************   StdYield    ***************************************
                    // Securities
                    try
                    {
                        sp = Constants.StdYield;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = exchangesString;
                        command.Parameters.Add("@is_index", SqlDbType.Int).Value = 0;   // from tb_Securities
                        command.ExecuteNonQuery();
                        calcStdYieldSuccess = true;
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull securities standard yields calculation", 0, null);
                    }
                    catch (Exception ex)
                    {
                        calcStdYieldSuccess = false;
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull securities standard yields calculation", 0, ex);
                    }
                    //---------------------------------------
                    // Indices
                    try
                    {
                        sp = Constants.StdYield;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = "1,-2";    //exchangesString;
                        command.Parameters.Add("@is_index", SqlDbType.Int).Value = 1;   // from tbl_Indices
                        command.ExecuteNonQuery();
                        //calcStdYieldSuccess = true;
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull indices standard yields calculation", 0, null);
                    }
                    catch (Exception ex)
                    {
                        //calcStdYieldSuccess = false;
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull indices standard yields calculation", 0, ex);
                    }
                    // ****************************************************************************************

                    // ******************************    MonetaryAvg        ***********************************
                    // Securities
                    try
                    {
                        sp = Constants.MonetaryAvg;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = exchangesString;
                        command.ExecuteNonQuery();
                        calcMonetaryAvgSuccess = true;
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull monetary average calculation", 0, null);
                    }
                    catch (Exception ex)
                    {
                        calcMonetaryAvgSuccess = false;
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull monetary average calculation", 0, ex);
                    }
                    // ****************************************************************************************
                }

                if (calcAvgYieldSuccess && calcStdYieldSuccess && calcMonetaryAvgSuccess)
                {
                    try
                    {
                        sp = Constants.SecurityRanking;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        command.Parameters.Add("@choose_for_exchange", SqlDbType.Int).Value = chooseSecuritiesForexchange;
                        command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = exchangesString;
                        command.ExecuteNonQuery();
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull securities ranking calculation", 0, null);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull securities ranking calculation", 0, ex);
                    }

                    // Insert calculated values into tbl_SecuritiesCalcHistory
                    try
                    {
                        sp = Constants.SecurityCalcHistory;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = exchangesString;
                        command.Parameters.Add("@is_index", SqlDbType.Int).Value = 0;   // from tb_Securities
                        command.ExecuteNonQuery();
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull Securities history calculation", 0, null);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull Securities history calculation", 0, ex);
                    }

                    // Insert calculated values into tbl_IndexCalcHistory
                    try
                    {
                        sp = Constants.SecurityCalcHistory;
                        SqlCommand command = new SqlCommand(string.Empty, conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = sp;
                        command.CommandTimeout = Constants.SPsTimeout;
                        command.Parameters.Add("@exchanges_list", SqlDbType.VarChar).Value = "1,-2";  // We have Tel Aviv indexes with 1, USA with '-2' value for exchange id
                        command.Parameters.Add("@is_index", SqlDbType.Int).Value = 1;   // from tbl_Indices
                        command.ExecuteNonQuery();
                        logger.WriteLog(Constants.Success, null, DateTime.MinValue, sp, "Successfull Index history calculation", 0, null);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog(Constants.Failure, null, DateTime.MinValue, sp, "Unsuccessfull Index history calculation", 0, ex);
                    }
                }
            }
        }
    }
}
