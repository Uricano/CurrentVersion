using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

// used namespaces
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.Protection;
using Cherries.TFI.BusinessLogic.Constraints;
using Cherries.TFI.BusinessLogic.DataManagement.ImportPorts;
using Cherries.TFI.BusinessLogic.Tools;
using Cherries.Models.App;

namespace Cherries.TFI.BusinessLogic.DataManagement
{
    public static class cSqlStatements
    {

        #region SQL Statement Methods

        #region Securities

        #region SELECT Securities

        public static String getSecuritiesList(List<DataRow> lstSecs)
        { // SELECT securities from a specified collection
            string listSecIds = "";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT tbl_Securities.*              ");
            sb.AppendLine("FROM tbl_Securities                  ");
            sb.AppendLine("WHERE  idSecurity in  ({0})          ");

            for (int iRows = 0; iRows < lstSecs.Count ; iRows++)
                listSecIds += string.Format("'{0}',", lstSecs[iRows]["idSecurity"].ToString());

            if (listSecIds.Length > 0)
                listSecIds = listSecIds.Substring(0, listSecIds.Length - 1);

            return string.Format(sb.ToString(), listSecIds);
        }//getSecuritiesList

        public static String getAllSecuritiesSortedSQL()
        { return "SELECT tbl_Securities.* FROM tbl_Securities ORDER BY " + cProperties.DisplayFieldName + " DESC;"; }//getAllSecuritiesSortedSQL

        #endregion SELECT Securities

        #region INSERT / UPDATE Securities

        public static String insertSecurityToDbSQL(cSecurity cNewSec)
        { // Inserts a given security to DB
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INSERT INTO tbl_Securities (idSecurity, strName, strSymbol, idSector, idSecurityType, idMarket                ");
            sb.AppendLine("                            , dtPriceStart, dtPriceEnd, strISIN, FAC                                          ");
            sb.AppendLine("                            , avgYield, stdYield, avgYieldNIS, stdYieldNIS                                    ");
            sb.AppendLine("                            , dValueUSA, dValueNIS, WeightUSA, WeightNIS                                      ");
            sb.AppendLine("                            , idCurrency, HebName)                                                            ");
            sb.AppendLine("                    VALUES ('{0}', '{1}', '{2}', {3}, {4}, {5}, '{6}', '{7}', '{8}', '{9}', '{10}'            ");
            sb.AppendLine("                             , '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}')        ");

            string sqlStatement = string.Format(sb.ToString(), cNewSec.Properties.PortSecurityId, cNewSec.Properties.SecurityName,
                                                               cNewSec.Properties.SecuritySymbol,
                                                               cNewSec.Properties.Sector.ID.ToString(),
                                                               cNewSec.Properties.SecurityType.ID.ToString(),
                                                               cNewSec.Properties.Market.ID.ToString(),
                                                               cGeneralFunctions.getDateFormatStrForSQL(cNewSec.DateRange.StartDate, "-"),
                                                               cGeneralFunctions.getDateFormatStrForSQL(cNewSec.DateRange.EndDate, "-"),
                                                               cNewSec.Properties.ISIN, cNewSec.FAC,
                                                               cNewSec.AvgYield, cNewSec.StdYield,
                                                               cNewSec.AvgYieldNIS, cNewSec.StdYieldNIS,
                                                               cNewSec.ValueUSA, cNewSec.ValueNIS,
                                                               cNewSec.WeightUSA, cNewSec.WeightNIS,
                                                               cNewSec.IdCurrency, cNewSec.Properties.HebName);

            return sqlStatement;
        }//insertSecurityToDbSQL

        public static String insertSecurityToDbSQL(DataRow drSec)
        { // Inserts a given security to DB
            double AvgYield = 0; double AvgYieldNIS = 0;
            double StdYield = 0; double StdYieldNIS = 0;
            double ValueUSA = 0; double ValueNIS = 0;
            double WeightUSA = 0; double WeightNIS = 0;
            string ISIN = "";

            string idCurrency = (drSec["idCurrency"] == DBNull.Value && drSec["idMarket"].ToString() == "-2") ? "9001" : drSec["idCurrency"].ToString();

            if (drSec["avgYield"] != DBNull.Value) AvgYield = Convert.ToDouble(drSec["avgYield"]);
            if (drSec["stdYield"] != DBNull.Value) StdYield = Convert.ToDouble(drSec["stdYield"]);

            if (drSec["avgYieldNIS"] != DBNull.Value) AvgYieldNIS = Convert.ToDouble(drSec["avgYieldNIS"]);
            if (drSec["stdYieldNIS"] != DBNull.Value) StdYieldNIS = Convert.ToDouble(drSec["stdYieldNIS"]);

            if (drSec["dValueUSA"] != DBNull.Value) ValueUSA = Convert.ToDouble(drSec["dValueUSA"]);
            if (drSec["dValueNIS"] != DBNull.Value) ValueNIS = Convert.ToDouble(drSec["dValueNIS"]);

            if (drSec["WeightUSA"] != DBNull.Value) WeightUSA = Convert.ToDouble(drSec["WeightUSA"]);
            if (drSec["WeightNIS"] != DBNull.Value) WeightNIS = Convert.ToDouble(drSec["WeightNIS"]);

            if (drSec["strISIN"] != DBNull.Value) ISIN = drSec["strISIN"].ToString();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INSERT INTO tbl_Securities (idSecurity, strName, strSymbol, idSector, idSecurityType, idMarket                ");
            sb.AppendLine("                            , dtPriceStart, dtPriceEnd, strISIN, FAC                                          ");
            sb.AppendLine("                            , avgYield, stdYield, avgYieldNIS, stdYieldNIS                                    ");
            sb.AppendLine("                            , dValueUSA, dValueNIS, WeightUSA, WeightNIS                                      ");
            sb.AppendLine("                            , idCurrency, HebName)                                                            ");
            sb.AppendLine("                    VALUES ('{0}', '{1}', '{2}', {3}, {4}, {5}, '{6}', '{7}', '{8}', '{9}', '{10}'            ");
            sb.AppendLine("                             , '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}')        ");

            string sqlStatement = string.Format(sb.ToString(), drSec["idSecurity"].ToString(),
                                                               (drSec["strName"].ToString()).Replace("'", "").Trim(),
                                                               drSec["strSymbol"].ToString(),
                                                               drSec["idSector"].ToString(),
                                                               drSec["idSecurityType"].ToString(),
                                                               drSec["idMarket"].ToString(),
                                                               cGeneralFunctions.getDateFormatStrForSQL(DateTime.Today.AddYears(-cProperties.DatesInterval), "-"),
                                                               cGeneralFunctions.getDateFormatStrForSQL(DateTime.Today.AddDays(-1), "-"),
                                                               ISIN, 1,      //FAC
                                                               AvgYield, StdYield,
                                                               AvgYieldNIS, StdYieldNIS,
                                                               ValueUSA, ValueNIS,
                                                               WeightUSA, WeightNIS,
                                                               idCurrency, drSec["HebName"].ToString());    //cProperties.CurrencyId

            return sqlStatement;
        }//insertSecurityToDbSQL

        public static String updtSecurityParametersToDbSQL(String idSec, object FAC, object avgYield, object stdYield, object avgYieldNIS, object stdYieldNIS
                                                           , object dValueUSA, object dValueNIS, object weightUSA, object weightNIS, DateTime dtEnd)  
        { // Updates the current security's parameters to DB
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("UPDATE tbl_Securities SET dtPriceEnd='{0}', FAC='{1}'                                                ");
            sb.AppendLine("                        , avgYield='{2}',  stdYield='{3}',  avgYieldNIS='{4}', stdYieldNIS='{5}'     ");
            sb.AppendLine("                        , dValueUSA='{6}', dValueNIS='{7}', WeightUSA='{8}',   WeightNIS='{9}'       ");
            sb.AppendLine("WHERE idSecurity='{10}'; ");
            string sqlStatement = string.Format(sb.ToString(), cGeneralFunctions.getDateFormatStrForSQL(dtEnd, "-"), FAC
                                                , avgYield,  stdYield,  avgYieldNIS, stdYieldNIS
                                                , dValueUSA, dValueNIS, weightUSA,   weightNIS, idSec);
            return sqlStatement;
        }//updtSecurityParametersToDbSQL

        #endregion INSERT / UPDATE Securities

        #endregion Securities

        #region Portfolio Securities

        #region SELECT Port Secs

        public static String getPortfoliosSectorStats()
        { // Retrieves the statistics for the sectors of all Real-Portfolios
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT        tbl_Portfolios.idPortfolio, tbl_Portfolios.strName, tblSel_Sectors.idSector, tblSel_Sectors.strName AS SectorName, ");
            sb.AppendLine(" COUNT(tbl_PortfolioSecurities.idSecurity) AS NumSecs                                                        ");
            sb.AppendLine(" FROM            tbl_Portfolios INNER JOIN                                                                   ");
            sb.AppendLine("tbl_PortfolioSecurities ON tbl_Portfolios.idPortfolio = tbl_PortfolioSecurities.idPortfolio INNER JOIN       ");
            sb.AppendLine("tbl_Securities ON tbl_PortfolioSecurities.idSecurity = tbl_Securities.idSecurity INNER JOIN                  ");
            sb.AppendLine("tblSel_Sectors ON tbl_Securities.idSector = tblSel_Sectors.idSector                                          ");
            sb.AppendLine("WHERE        (tbl_PortfolioSecurities.flWeight > 0)                                                          ");
            sb.AppendLine("GROUP BY tbl_Portfolios.idPortfolio, tbl_Portfolios.strName, tblSel_Sectors.idSector, tblSel_Sectors.strName ");

            string sqlStatement = string.Format(sb.ToString(), DateTime.Today.AddDays(-1).ToString("yyyy/MM/dd"));

            return sqlStatement;
        }//getRealPortfoliosSectorStats

        //public static String getPortFinalDetailsSecsByPortIdSQL(string iPortId, string calcCurrency)
        //{ // Retrieves all securities belonging to a certain portfolio
        //    // NOT USED FOR NOW, BUT DON'T DELETE, IS USED AS A TEMPLATE
        //    StringBuilder sb = new StringBuilder();
        //    int agorotAdj = 1;
        //    string fldName = "fClose";
        //    string stdFldName = "stdYield";
        //    if (calcCurrency == "ILS")
        //    {
        //        fldName = "fNISClose";
        //        stdFldName = "stdYieldNIS";
        //        agorotAdj = 100;
        //    }

        //    sb.AppendLine("SELECT  PS.idSecurity , PS.flWeight , PS.flRisk, PS.flQuantity                                                                 ");
        //    sb.AppendLine("      , (Case when Pr.{2} is NULL then 0 else Pr.{2} end) / (Case when S.FAC is NULL then 1 else S.FAC end) as fClose          ");   /// because the price is in agorot for israeli securities, there is no division by 100
        //    sb.AppendLine("      , S.dtPriceEnd as LastTradingDate                                                                                        ");
        //    sb.AppendLine("      , S.strSymbol  , S.strName , S.HebName                                                                                   ");
        //    sb.AppendLine("      , PS.flQuantity * (Case when Pr.{2} is NULL then 0 else Pr.{2} / {3} end) / (Case when S.FAC is NULL then 1 else S.FAC end) as SecValue"); /// (CASE WHEN S.idCurrency = 'ILS' THEN 100 ELSE 1 END)
        //    sb.AppendLine("      , S.idMarket, S.idSector, S.idSecurityType                                                                               ");
        //    //sb.AppendLine("      , Sec.strName as strSectorName, SM.strName as strMarketName                                                            ");
        //    sb.AppendLine("      , S.idCurrency                                                                                                           ");
        //    sb.AppendLine("      , S.{4} * sqrt(52)  as StdDev                                                                                            ");
        //    sb.AppendLine("FROM tbl_PortfolioSecurities as PS                                                                                             ");
        //    sb.AppendLine("INNER JOIN tbl_Securities as S  ON PS.idSecurity = S.idSecurity                                                                ");
        //    sb.AppendLine("Left outer join tbl_Prices AS Pr ON PS.idSecurity = Pr.idSecurity                                                              ");
        //    sb.AppendLine("WHERE PS.idPortfolio = {0} and PS.flWeight > 0   and PS.isActiveSecurity = 1                                                   ");
        //    sb.AppendLine("      and Pr.dDate = '{1}'                                                                                                     ");
        //    sb.AppendLine("ORDER BY S.strName                                                                                                             ");

        //    string sqlStatement = string.Format(sb.ToString(), iPortId, DateTime.Today.AddDays(-1).ToString("yyyy/MM/dd"), fldName, agorotAdj, stdFldName);

        //    return sqlStatement;
        //}//getSecuritiesByBaseSQL

        public static String getPortSecuritiesForConstructionByPortId(int iPortId, string calcCurrency)
        { // Retrieves all securities belonging to a certain portfolio

            string fldName = (calcCurrency == "ILS") ? "WeightNIS" : "WeightUSA";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT  PS.idSecurity                                                                   ");
            sb.AppendLine("      , S.strSymbol, S.strName, S.HebName                                               ");
            sb.AppendLine("      , S.idMarket, S.idSector, S.idSecurityType                                        ");
            sb.AppendLine("      , S.idCurrency                                                                    ");
            sb.AppendLine("      , S.stdYield * SQRT(52) AS stdYield, S.avgYield * 52 AS avgYield                  ");
            sb.AppendLine("      , S.stdYieldNIS * SQRT(52) AS stdYieldNIS,  S.avgYieldNIS * 52 AS avgYieldNIS     ");
            sb.AppendLine("      , S.dValueUSA, S.dValueNIS                                                        ");
            sb.AppendLine("      , S.WeightUSA, S.WeightNIS                                                        ");
            sb.AppendLine("      , SM.StockMarketShortName as marketName, ST.SecurityTypeEngName as securityTypeName, SCT.SectorEngName  as sectorName  ");
            sb.AppendLine("      , isActiveSecurity as isSelected                                                  ");
            sb.AppendLine("FROM tbl_PortfolioSecurities as PS                                                      ");
            sb.AppendLine("INNER JOIN v_Securities as S  ON PS.idSecurity = S.idSecurity                         ");
            sb.AppendLine("INNER JOIN tbSel_StockMarkets as SM  ON SM.StockMarketID = S.idMarket                  ");
            sb.AppendLine("INNER JOIN tbSel_SecurityTypes as ST  ON ST.SecurityType = S.idSecurityType        ");
            sb.AppendLine("INNER JOIN tbSel_Sectors as SCT  ON SCT.Sector = S.idSector                          ");
            sb.AppendLine("WHERE PS.idPortfolio = {0}                                                              ");
            sb.AppendLine("ORDER BY S.{1} DESC;                                                                        ");

            string sqlStatement = string.Format(sb.ToString(), iPortId.ToString(), fldName);
            return sqlStatement;
        }//getPortSecuritiesForConstructionByPortId

        public static String getSecuritiesByPortIdSQL(int iPortId)
        { // Retrieves all securities belonging to a certain Securities Base
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT PS.* , S.*                                                   ");
            sb.AppendLine("FROM tbl_PortfolioSecurities  PS                                    ");
            sb.AppendLine("INNER JOIN tbl_Securities     S ON PS.idSecurity = S.idSecurity     ");
            sb.AppendLine("WHERE PS.idPortfolio = {0}                                          ");
            sb.AppendLine("ORDER BY S.{1} ;                                                    ");

            string sqlStatement = string.Format(sb.ToString(), iPortId.ToString(), cProperties.DisplayFieldName);
            return sqlStatement;
        }//getSecuritiesByBaseSQL

        #endregion SELECT Port Secs

        #region INSERT / UPDATE Port Secs

        public static String insertPortfolioSecurityToDbSQL(cSecurity cNewSec, int iPortId)
        { // Inserts a given portfolio security to DB
            int iActiveVal = cNewSec.isActive ? 1 : 0;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INSERT INTO tbl_PortfolioSecurities (idPortfolio, idSecurity, dtStartDate, dtEndDate, flQuantity, flWeight, flRisk, flRate, flLastPrice, isActiveSecurity) ");
            sb.AppendLine("                    VALUES ( {0} ,                                                                                                            ");
            sb.AppendLine("                            '{1}',                                                                                                            ");
            sb.AppendLine("                            '{2}',                                                                                                            ");
            sb.AppendLine("                            '{3}',                                                                                                            ");
            sb.AppendLine("                            '{4}',                                                                                                            ");
            sb.AppendLine("                            '{5}',                                                                                                            ");
            sb.AppendLine("                             {6} ,                                                                                                            ");
            sb.AppendLine("                             {7} ,                                                                                                            ");
            sb.AppendLine("                             {8} ,                                                                                                            ");
            sb.AppendLine("                             {9} )                                                                                                            ");

            string sqlStatement = string.Format(sb.ToString(), iPortId.ToString(),
                                                               cNewSec.Properties.PortSecurityId.ToString(),
                                                               cGeneralFunctions.getDateFormatStrForSQL(cNewSec.DateRange.StartDate, "-"),
                                                               cGeneralFunctions.getDateFormatStrForSQL(cNewSec.DateRange.EndDate, "-"),
                                                               cNewSec.Quantity,
                                                               cNewSec.Weight.ToString(),
                                                               cNewSec.CovarClass.StandardDeviation.ToString(),
                                                               cNewSec.RatesClass.FinalRate.ToString(),
                                                               cNewSec.LastPrice.ToString(),
                                                               iActiveVal.ToString());
            return sqlStatement;
        }//insertPortfolioSecurityToDbSQL

        public static String insertPortfolioSecurityWithIdToDbSQL(cSecurity cNewSec, int iPortId, String idSec)
        { // Inserts a given portfolio security to DB
            int iActiveVal = cNewSec.isActive ? 1 : 0;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INSERT INTO tbl_PortfolioSecurities (idPortfolio, idSecurity, dtStartDate, dtEndDate, flQuantity, flWeight, flRisk, flRate, flLastPrice, isActiveSecurity) ");
            sb.AppendLine("                    VALUES ( {0} ,                                                                                                            ");
            sb.AppendLine("                            '{1}',                                                                                                            ");
            sb.AppendLine("                            '{2}',                                                                                                            ");
            sb.AppendLine("                            '{3}',                                                                                                            ");
            sb.AppendLine("                            '{4}',                                                                                                            ");
            sb.AppendLine("                            '{5}',                                                                                                            ");
            sb.AppendLine("                             {6} ,                                                                                                            ");
            sb.AppendLine("                             {7} ,                                                                                                            ");
            sb.AppendLine("                             {8} ,                                                                                                            ");
            sb.AppendLine("                             {9} )                                                                                                            ");

            string sqlStatement = string.Format(sb.ToString(), iPortId.ToString(), idSec.ToString(),
                                                               cGeneralFunctions.getDateFormatStrForSQL(DateTime.Today.AddYears(-cProperties.DatesInterval), "-"),
                                                               cGeneralFunctions.getDateFormatStrForSQL(DateTime.Today.AddDays(-1), "-"),
                                                               cNewSec.Quantity, cNewSec.Weight,
                                                               cNewSec.CovarClass.StandardDeviation.ToString(), cNewSec.RatesClass.FinalRate.ToString(),
                                                               cNewSec.LastPrice.ToString(),
                                                               iActiveVal.ToString());
            return sqlStatement;
        }//insertPortfolioSecurityToDbSQL

        public static String clearPortfolioSecurities(int idPortfolio)
        { return string.Format("DELETE FROM tbl_PortfolioSecurities WHERE idPortfolio = '{0}';", idPortfolio.ToString()); }//clearPortfolioSecurities

        #endregion INSERT / UPDATE Port Secs

        #endregion Portfolio Securities

        #region Prices

        public static String getSecurityPrices(cDateRange dtRange, String idSec)
        { // Returns the SQL statement necessary for retrieving the prices of the portfolio
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(" SELECT  P.idSecurity                                                                                       ");
            sb.AppendLine("       , P.dDate                                                                                            ");
            sb.AppendLine("       , P.fClose                                                                                           ");
            sb.AppendLine("       , P.fNISClose                                                                                        ");
            sb.AppendLine("       , P.fOpen                                                                                            ");
            sb.AppendLine("       , P.fNISOpen                                                                                         ");
            sb.AppendLine("       , P.isHoliday                                                                                        ");
            sb.AppendLine("       , COALESCE(P.FAC, 1.0) as FAC                                                                        ");
            sb.AppendLine(" FROM tbl_Prices P                                                                                          ");
            sb.AppendLine(" WHERE (P.idSecurity = '{0}')                                                                               ");
            sb.AppendLine(" ORDER BY dDate DESC;                                                                                       ");

            string sqlStatement = string.Format(sb.ToString(), idSec.ToString());
            return sqlStatement;

        }//getSecurityPrices

        public static String getSecurityAdjustedPrices(cDateRange dtRange, String idSec)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" SELECT P.idSecurity                                      ");
            sb.AppendLine("       , P.dDate                                          ");
            sb.AppendLine("       , P.fClose                                         ");
            sb.AppendLine("       , P.fOpen                                          ");
            sb.AppendLine("       , P.fNISClose                                      ");
            sb.AppendLine("       , P.fNISOpen                                       ");
            sb.AppendLine("       , COALESCE(P.FAC, 1.0) as FAC                      ");
            sb.AppendLine(" FROM tbl_Prices P                                        ");
            sb.AppendLine(" WHERE P.idSecurity = '{0}' AND                           ");
            sb.AppendLine("       P.fClose IS NOT NULL AND                           ");
            sb.AppendLine("       P.dDate Between '{1}' AND '{2}'                    ");
            sb.AppendLine(" ORDER BY dDate DESC;                                     ");

            string sqlStatement = string.Format(sb.ToString(), idSec.ToString(), 
                cGeneralFunctions.getDateFormatStrForSQL(dtRange.StartDate, "/"), 
                cGeneralFunctions.getDateFormatStrForSQL(dtRange.EndDate, "/"));
            return sqlStatement;
        }//getSecurityAdjustedPrices

        public static String getActiveSecurityPrices(cSecurities cSecsCol, cDateRange cDr, Boolean isActive)
        { // SELECT active securities in collection FROM local prices Table
            /// Params: isActive - Whether to consider only active securities
            string listSecIds = "";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" SELECT        idSecurity, dDate, fVolume, fClose, FAC, fNISClose, isHoliday     ");
            sb.AppendLine(" FROM            tbl_Prices                                                      ");
            sb.AppendLine(" WHERE (dDate BETWEEN CONVERT(DATETIME, '{0}', 102) AND CONVERT(DATETIME, '{1}', 102)) ");
            sb.AppendLine(" AND   (idSecurity in  ({2}))                                                    ");
            sb.AppendLine(" ORDER BY dDate, idSecurity;                                                     ");

            for (int iRows = 0; iRows < cSecsCol.Count; iRows++)
                if ((!isActive) || (isActive && (cSecsCol[iRows].Weight > 0D)))
                    listSecIds += string.Format("'{0}',", cSecsCol[iRows].Properties.PortSecurityId);

            if (listSecIds.Length > 0)
                listSecIds = listSecIds.Substring(0, listSecIds.Length - 1);

            return string.Format(sb.ToString(), cGeneralFunctions.getDateFormatStrForSQL(cDr.StartDate, "-"), cGeneralFunctions.getDateFormatStrForSQL(cDr.EndDate, "-"), listSecIds);
        }//getActiveSecurityPrices

        //public static String deletePreviousSecPrices(cDateRange dtRange, String iSecId)
        //{ // Deletes previous prices for security
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("DELETE FROM tbl_Prices                                             ");
        //    sb.AppendLine("WHERE tbl_Prices.idSecurity = '{0}' AND                            ");
        //    sb.AppendLine("      tbl_Prices.dDate Between '{1}' And '{2}';                    ");

        //    string sqlStatement = string.Format(sb.ToString(), iSecId.ToString(),
        //                                                       cGeneralFunctions.getDateFormatStrForSQL(dtRange.StartDate, "/"),
        //                                                       cGeneralFunctions.getDateFormatStrForSQL(dtRange.EndDate, "/"));
        //    return sqlStatement;
        //}//deletePreviousSecPrices

        public static String deleteDbOldPrices(DateTime dtBelow)
        { // Deletes old prices from database (earlier than the dates interval specified)
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("DELETE FROM tbl_Prices                                             ");
            sb.AppendLine("WHERE tbl_Prices.dDate < '{0}';                    ");

            String sqlStatement = string.Format(sb.ToString(), cGeneralFunctions.getDateFormatStrForSQL(dtBelow, "/"));
            return sqlStatement;
        }//deletePortfolioOldPrices

        #endregion Prices

        #region Portfolio

        #region SELECT Portfolio

        public static String getAllPortfolios(int userId)
        { // Retrieves all portfolios 
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT                                                                                                                 ");
            sb.AppendLine("    port.idPortfolio,                                                                                                  ");
            sb.AppendLine("    port.userId,                                                                                                  ");
            sb.AppendLine("    min(port.strName) as strName,                                                                                      ");
            sb.AppendLine("    min(port.strCode) as strCode,                                                                                      ");
            sb.AppendLine("    min(port.dEquity) as dEquity,                                                                                      ");
            sb.AppendLine("    min(port.dCurrEquity) as currentEquity,                                                                                      ");
            sb.AppendLine("    min(port.dInitRisk) as dInitRisk,                                                                                  ");
            sb.AppendLine("    min(port.dCurrRisk) as dCurrRisk,                                                                                  ");
            sb.AppendLine("    min(port.iMaxSecs) as iMaxSecs,                                                                                    ");
            sb.AppendLine("    min(port.iCalcPreference) as iCalcPreference,                                                                      ");
            sb.AppendLine("    min(prices.fClose) as PriceYesterday,                                                                              ");
            sb.AppendLine("    min(port.dtCreated) as dtCreated,                                                                                  ");
            sb.AppendLine("    min(port.dtLastOptimization) as dtLastOptimization,                                                                ");
            sb.AppendLine("    min(port.CalcCurrency) as CalcCurrency                                                                             ");
            sb.AppendLine("    ,                                                                                                                  ");
            sb.AppendLine("    case min(port.dtLastOptimization) when '" + DateTime.Today.ToShortDateString() + "' then 0                                              ");
            sb.AppendLine("         else                                                                                                          ");
            sb.AppendLine("           case min(port.CalcCurrency)                                                                                 ");
            sb.AppendLine("             when '9001' then                                                                                          ");
            sb.AppendLine("               case SUM (ps.flQuantity * prices.fClose) when 0 then 0                                                  ");
            sb.AppendLine("                 else SUM (ps.flQuantity * prices.fClose) - min(port.dCurrEquity)                                      ");
            sb.AppendLine("               end                                                                                                     ");
            sb.AppendLine("             else                                                                                                      ");
            sb.AppendLine("               case SUM (ps.flQuantity * prices.fNISClose) when 0 then 0                                               ");
            sb.AppendLine("                 else (SUM (ps.flQuantity * prices.fNISClose) - min(port.dCurrEquity)) / 10000                         ");
            sb.AppendLine("               end                                                                                                     ");
            sb.AppendLine("           end                                                                                                         ");
            sb.AppendLine("    end as dLastProfit                                                                                                 ");
            sb.AppendLine("    ,                                                                                                                  ");
            sb.AppendLine("    min(port.iSecsNum) as iSecsNum                                                                                     ");
            sb.AppendLine("  FROM                                                                                                                 ");
            sb.AppendLine("    (                                                                                                                  ");
            sb.AppendLine("      tbl_Portfolios port                                                                                              ");
            sb.AppendLine("    LEFT OUTER JOIN                                                                                                    ");
            sb.AppendLine("      tbl_PortfolioSecurities ps                                                                                       ");
            sb.AppendLine("    ON                                                                                                                 ");
            sb.AppendLine("      port.idPortfolio = ps.idPortfolio                                                                                ");
            sb.AppendLine("    )                                                                                                                  ");
            sb.AppendLine("    LEFT OUTER JOIN                                                                                                    ");
            sb.AppendLine("      tbl_Prices prices                                                                                                ");
            sb.AppendLine("    ON                                                                                                                 ");
            sb.AppendLine("      prices.dDate = '" + DateTime.Today.AddDays(-1).ToShortDateString() + "'                                                                       ");
            sb.AppendLine("      and                                                                                                              ");
            sb.AppendLine("      ps.idSecurity = prices.idSecurity                                                                                ");
            sb.AppendLine("  WHERE iSecsNum > 0                                                                                                   ");
            sb.AppendLine("  GROUP BY                                                                                                             ");
            sb.AppendLine("    port.idPortfolio                                                                                                   ");
            sb.AppendLine("  ORDER BY                                                                                                             ");
            sb.AppendLine("    port.idPortfolio;                                                                                                  ");

            return sb.ToString();
        }//getAllPortfolios

        //public static String getAllPortfolios()
        //{ // Retrieves all portfolios 
        //    String strYesterdaysDate = DateTime.Today.AddDays(-1).ToString("yyyy/MM/dd");
        //    String strTodaysDate = DateTime.Today.ToString("yyyy/MM/dd");

        //    StringBuilder sb = new StringBuilder();

        //    sb.AppendLine("SELECT                                                                                                                 ");
        //    sb.AppendLine("    port.idPortfolio,                                                                                                  ");
        //    sb.AppendLine("    min(port.strName) as strName,                                                                                      ");
        //    sb.AppendLine("    min(port.dEquity) as dEquity,                                                                                      ");
        //    sb.AppendLine("    min(port.dCurrEquity) as currentEquity,                                                                                      ");
        //    sb.AppendLine("    min(port.dInitRisk) as dInitRisk,                                                                                  ");
        //    sb.AppendLine("    min(port.dCurrRisk) as dCurrRisk,                                                                                  ");
        //    sb.AppendLine("    min(port.iMaxSecs) as iMaxSecs,                                                                                    ");
        //    sb.AppendLine("    min(port.iCalcPreference) as iCalcPreference,                                                                      ");
        //    sb.AppendLine("    min(prices.fClose) as PriceYesterday,                                                                              ");
        //    sb.AppendLine("    min(port.dtCreated) as dtCreated,                                                                                  ");
        //    sb.AppendLine("    min(port.dtLastOptimization) as dtLastOptimization,                                                                ");
        //    sb.AppendLine("    min(port.CalcCurrency) as CalcCurrency                                                                             ");
        //    sb.AppendLine("    ,                                                                                                                  ");
        //    sb.AppendLine("    case min(port.dtLastOptimization) when '" + strTodaysDate + "' then 0                                          ");
        //    sb.AppendLine("         else                                                                                                             ");
        //    sb.AppendLine("             case SUM (ps.flQuantity * prices.fClose) when 0 then 0                                            ");
        //    sb.AppendLine("             else SUM (ps.flQuantity * prices.fClose) - min(port.dCurrEquity)                                             ");
        //    sb.AppendLine("             end                                             ");
        //    sb.AppendLine("    end as dLastProfit                                                                                                 ");
        //    sb.AppendLine("    ,                                                                                                                  ");
        //    sb.AppendLine("    min(port.iSecsNum) as iSecsNum                                                                                     ");
        //    sb.AppendLine("  FROM                                                                                                                 ");
        //    sb.AppendLine("    (                                                                                                                  ");
        //    sb.AppendLine("      tbl_Portfolios port                                                                                              ");
        //    sb.AppendLine("    LEFT OUTER JOIN                                                                                                    ");
        //    sb.AppendLine("      tbl_PortfolioSecurities ps                                                                                       ");
        //    sb.AppendLine("    ON                                                                                                                 ");
        //    sb.AppendLine("      port.idPortfolio = ps.idPortfolio                                                                                ");
        //    sb.AppendLine("    )                                                                                                                  ");
        //    sb.AppendLine("    LEFT OUTER JOIN                                                                                                    ");
        //    sb.AppendLine("      tbl_Prices prices                                                                                                ");
        //    sb.AppendLine("    ON                                                                                                                 ");
        //    sb.AppendLine("      prices.dDate = '" + strYesterdaysDate + "'                                                                       ");
        //    sb.AppendLine("      and                                                                                                              ");
        //    sb.AppendLine("      ps.idSecurity = prices.idSecurity                                                                                ");
        //    sb.AppendLine("  WHERE iSecsNum > 0                                                                                                   ");
        //    sb.AppendLine("  GROUP BY                                                                                                             ");
        //    sb.AppendLine("    port.idPortfolio                                                                                                   ");
        //    sb.AppendLine("  ORDER BY                                                                                                             ");
        //    sb.AppendLine("    port.idPortfolio;                                                                                                  ");

        //    return sb.ToString();
        //}//getAllPortfolios

        public static String getAllNotImportedPortfolios()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT P.*,                                                                                                               ");
            sb.AppendLine("       SC.strDesc,                                                                                                        ");
            sb.AppendLine("       PGR.PortSecCount,                                                                                                  ");
            sb.AppendLine("       PGR.CurrentEquity                                                                                                  ");
            sb.AppendLine("FROM tbl_Portfolios P                                                                                                     ");
            sb.AppendLine("INNER JOIN [tblSel_Currency] SC on P.CalcCurrency = SC.idCurrency                                                         ");
            sb.AppendLine("LEFT OUTER JOIN (SELECT  PS.idPortfolio, COUNT(*) as PortSecCount,                                                        ");
            sb.AppendLine("                      SUM(PS.flQuantity * (Case when PP.CalcCurrency = '9001'                                             ");
            sb.AppendLine("                                                then (Case when Pr.fClose is NULL then 0 else Pr.fClose end)              ");
            sb.AppendLine("                                                else (Case when Pr.fNISClose is NULL then 0 else Pr.fNISClose / 100 end)  ");
            //sb.AppendLine("                                            end) / (Case when S.FAC is NULL then 1 else S.FAC end)) as CurrentEquity      ");
            sb.AppendLine("                                            end)) as CurrentEquity      ");
            //sb.AppendLine("                                                 / (CASE WHEN S.idCurrency = 'ILS' THEN 100 ELSE 1 END)) as CurrentEquity ");
            sb.AppendLine("                 FROM tbl_PortfolioSecurities as PS                                                                       ");
            sb.AppendLine("                 INNER JOIN    tbl_Portfolios AS PP on  PS.idPortfolio = PP.idPortfolio                                   ");
            sb.AppendLine("                 INNER JOIN tbl_Securities as S  ON PS.idSecurity = S.idSecurity                                          ");
            sb.AppendLine("                 Left outer join tbl_Prices AS Pr ON PS.idSecurity = Pr.idSecurity                                        ");
            sb.AppendLine("                 WHERE  PS.flWeight > 0   and PS.isActiveSecurity = 1   and Pr.dDate = '{0}'                              ");
            sb.AppendLine("                 GROUP BY  PS.idPortfolio                                                                                 ");
            sb.AppendLine("                 )   PGR on PGR.idPortfolio = P.idPortfolio                                                               ");
            sb.AppendLine("WHERE P.isImported = 0;                                                                                                   ");

            string sqlStatement = string.Format(sb.ToString(), DateTime.Today.AddDays(-1).ToString("yyyy/MM/dd"));


            return sqlStatement;
        }//getAllNotImportedPortfolios

        public static String getLastDollarValue()
        { return string.Format("SELECT fClose FROM tbl_Prices WHERE idSecurity = '{0}' ORDER BY dDate DESC;", 9001); }//getLastDollarValue

        #endregion SELECT Portfolio

        #region Portfolio INSERT / REMOVE methods

        public static String insertNewPortfolioSQL(PortfolioDetails cCurrPort)
        { // Inserts a given portfolio to DB
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INSERT INTO tbl_Portfolios (strCode, strName, dtLastOpened, dtCreated, dtStartDate, dtEndDate, dEquity, dCurrEquity, iSecsNum, isImported, dInitRisk, iMaxSecs, iCalcPreference, CalcCurrency)   ");
            sb.AppendLine("                    VALUES ('{0}',                                                                                         ");
            sb.AppendLine("                            '{1}',                                                                                         ");
            sb.AppendLine("                            '{2}',                                                                                         ");
            sb.AppendLine("                            '{3}',                                                                                         ");
            sb.AppendLine("                            '{4}',                                                                                         ");
            sb.AppendLine("                            '{5}',                                                                                         ");
            sb.AppendLine("                             {6} ,                                                                                         ");
            sb.AppendLine("                             {7} ,                                                                                         ");
            sb.AppendLine("                             0, 0,                                                                                         ");
            sb.AppendLine("                            '{8}',                                                                                         ");
            sb.AppendLine("                             {9},                                                                                         ");
            sb.AppendLine("                             {10},                                                                                          ");
            sb.AppendLine("                            '{11}' )                                                                                       ");

            string sqlStatement = string.Format(sb.ToString(), cCurrPort.Code, cCurrPort.Name,
                                                               cGeneralFunctions.getDateFormatStrForSQL(cCurrPort.DateEdited, "-"),
                                                               cGeneralFunctions.getDateFormatStrForSQL(cCurrPort.DateCreated, "-"),
                                                               cGeneralFunctions.getDateFormatStrForSQL(DateTime.Today.AddYears(-cProperties.DatesInterval).AddDays(-1), "-"),
                                                               cGeneralFunctions.getDateFormatStrForSQL(DateTime.Today.AddDays(-1), "-"),
                                                               cCurrPort.Equity.ToString(), cCurrPort.Equity.ToString(),
                                                               cCurrPort.PreferedRisk.UpperBound, 
                                                               cCurrPort.MaxSecs, (int)cCurrPort.CalcType, 
                                                               cCurrPort.CalcCurrency);
            return sqlStatement;
        }//insertNewPortfolioSQL

        public static String removePrevPortfolioSecuritiesSQL(int idPortfolio)
        { // Inserts a given portfolio to DB
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("DELETE FROM tbl_PortfolioSecurities               ");
            sb.AppendLine(" WHERE (tbl_PortfolioSecurities.idPortfolio = {0}) ");
            string sqlStatement = string.Format(sb.ToString(), idPortfolio);
            return sqlStatement;
        }//insertNewPortfolioSQL

        #endregion Portfolio INSERT / REMOVE methods

        #region Portfolio UPDATE methods
        
        //public static String updtPortfolioRiskLevel(double dRiskLevel, int idPortfolio)
        //{ // Updates the prefered risk level for the current portfolio
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("UPDATE   tbl_Portfolios         ");
        //    sb.AppendLine("SET dInitRisk =    '{0}'        ");
        //    sb.AppendLine("WHERE idPortfolio = {1}         ");

        //    string sqlStatement = string.Format(sb.ToString(), dRiskLevel, idPortfolio);
        //    return sqlStatement;
        //}//updtPortfolioRiskLevel

        //public static String updtPortfolioRisk_Equity_AndPreference(double dRiskLevel, double dEquity, int iCalcPref, int idPortfolio)
        //{ // Updates the prefered risk level for the current portfolio
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("UPDATE   tbl_Portfolios                 ");
        //    sb.AppendLine("SET dInitRisk =            '{0}'        ");
        //    sb.AppendLine("    , dEquity =            '{1}'        ");
        //    sb.AppendLine("    , iCalcPreference =     {2}         ");
        //    sb.AppendLine("WHERE idPortfolio =         {3}         ");

        //    string sqlStatement = string.Format(sb.ToString(), dRiskLevel, dEquity, iCalcPref, idPortfolio);
        //    return sqlStatement;
        //}//updtPortfolioRiskLevel

        public static String updtExtPortDetails(cImportedPort cExtPort)
        { // Updates the current external portfolio data to DB
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("UPDATE tbl_Portfolios           ");
            sb.AppendLine("SET dEquity      =  {0},        ");
            sb.AppendLine("    iSecsNum     =  {1}         ");
            sb.AppendLine("WHERE idPortfolio = {2}         ");

            string sqlStatement = string.Format(sb.ToString(), cExtPort.Equity, cExtPort.Securities.Count, cExtPort.ID.ToString());
            return sqlStatement;
        }//updtExtPortDetails

        public static String updtPortfolioEndDates(DateTime dtEndDate)
        { // Updates the last dates of all portfolios in DB
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("UPDATE tbl_Portfolios                          ");
            sb.AppendLine("SET dtEndDate = CONVERT(DATETIME, '{0}', 102); ");
            string sqlStatement = string.Format(sb.ToString(), cGeneralFunctions.getDateFormatStrForSQL(dtEndDate, "-"));
            return sqlStatement;
        }//updtPortfolioEndDates

        public static String updateExistingPortfolio(PortfolioDetails cCurrPort)
        { // Updates the current portfolio data to DB
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("UPDATE tbl_Portfolios       ");
            sb.AppendLine("SET strName = '{0}'         ");
            sb.AppendLine("  , strCode = '{1}'         ");
            sb.AppendLine("  , dEquity =  {2}          ");
            sb.AppendLine("  , dInitRisk = {3}         ");
            sb.AppendLine("  , iMaxSecs =  {4}         ");
            sb.AppendLine("  , CalcCurrency = '{5}'    ");
            sb.AppendLine("WHERE idPortfolio = {6}     ");

            string sqlStatement = string.Format(sb.ToString(), cCurrPort.Name
                                                             , cCurrPort.Code
                                                             , cCurrPort.Equity
                                                             , cCurrPort.PreferedRisk.UpperBound
                                                             , cCurrPort.MaxSecs
                                                             , cCurrPort.CalcCurrency
                                                             , cCurrPort.ID);
            return sqlStatement;
        }//updateExistingPortfolio

        #endregion Portfolio UPDATE methods

        #endregion Portfolio

        #region Real Portfolio

        //public static String insertNewRealPortfolioSQL(string realPortName, int secCount, double investmentAmount, double portRisk, string calcCurrency, Boolean isManual, enumEfCalculationType eCalcType)
        //{ // Inserts a given portfolio to DB
        //    StringBuilder sb = new StringBuilder();

        //    sb.AppendLine("INSERT INTO tbl_RealPortfolios (RealPortName, SecuritiesCount, CreatedDT, InvestmentAmount, Risk, CalcCurrency, isManual, idCalcType)            ");
        //    sb.AppendLine("                    VALUES ( '{0}', {1}, '{2}', {3}, {4}, '{5}', '{6}', {7}  )                                                                   ");

        //    string sqlStatement = string.Format(sb.ToString(), realPortName, secCount.ToString(), cGeneralFunctions.getDateFormatStrForSQL(DateTime.Now, "-"),
        //                                                       investmentAmount.ToString(), portRisk.ToString(), calcCurrency, isManual, Convert.ToInt32(eCalcType));
        //    return sqlStatement;
        //}//insertNewPortfolioSQL

        //public static String insertNewRealPortfolioSecuritiesSQL(int realPortID, DataRow dr)
        //{ // Inserts a given portfolio to DB
        //    StringBuilder sb = new StringBuilder();

        //    sb.AppendLine("INSERT INTO tbl_RealPortfolioSecurities (RealPortID, flWeight, idSecurity, flQuantity)          ");
        //    sb.AppendLine("                    VALUES ( {0},                                                                       ");
        //    sb.AppendLine("                             {1},                                                                       ");
        //    sb.AppendLine("                            '{2}',                                                                      ");
        //    sb.AppendLine("                             {3} )                                                                      ");

        //    string sqlStatement = string.Format(sb.ToString(), realPortID, Convert.ToDouble(dr["dWeight"]).ToString(), dr["securityId"].ToString(), Convert.ToDouble(dr["dQuantity"]).ToString());
        //    return sqlStatement;
        //}//insertNewPortfolioSQL

        //public static String removePrevPortfolioSecuritiesSQL(int realPortID)
        //{ // Inserts a given portfolio to DB
        //    StringBuilder sb = new StringBuilder();

        //    sb.AppendLine("DELETE FROM tbl_RealPortfolioSecurities               ");
        //    sb.AppendLine(" WHERE (tbl_RealPortfolioSecurities.RealPortID = {0}) ");
        //    string sqlStatement = string.Format(sb.ToString(), realPortID);
        //    return sqlStatement;
        //}//insertNewPortfolioSQL

        //public static String updtRealPortfolioDataSQL(int realPortID, int iSecsCount)
        //{ // Inserts a given portfolio to DB
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("UPDATE       tbl_RealPortfolios                        ");
        //    sb.AppendLine(" SET         SecuritiesCount = {0}                     ");
        //    sb.AppendLine(" WHERE       (tbl_RealPortfolios.RealPortID = {1})       ");

        //    string sqlStatement = string.Format(sb.ToString(), iSecsCount, realPortID);
        //    return sqlStatement;
        //}//insertNewPortfolioSQL

        //public static String getAllRealPortfolios()
        //{
        //    StringBuilder sb = new StringBuilder();

        //    sb.AppendLine("SELECT P.*,                                                                                                               ");
        //    sb.AppendLine("       SC.strDesc,                                                                                                        ");
        //    sb.AppendLine("       PGR.PortSecCount,                                                                                                  ");
        //    sb.AppendLine("       PGR.CurrentEquity                                                                                                  ");
        //    sb.AppendLine("FROM tbl_RealPortfolios P                                                                                                 ");
        //    sb.AppendLine("INNER JOIN [tblSel_Currency] SC on P.CalcCurrency = SC.idCurrency                                                         ");
        //    sb.AppendLine("LEFT OUTER JOIN (SELECT  PS.RealPortID, COUNT(*) as PortSecCount,                                                         ");
        //    sb.AppendLine("                      SUM(PS.flQuantity * (Case when PP.CalcCurrency = '9001'                                             ");
        //    sb.AppendLine("                                                then (Case when Pr.fClose is NULL then 0 else Pr.fClose end)              ");
        //    sb.AppendLine("                                                else (Case when Pr.fNISClose is NULL then 0 else Pr.fNISClose / 100 end)  ");
        //    sb.AppendLine("                                            end) / (Case when S.FAC is NULL then 1 else S.FAC end)) as CurrentEquity      ");
        //    sb.AppendLine("                 FROM tbl_RealPortfolioSecurities as PS                                                                   ");
        //    sb.AppendLine("                 INNER JOIN    tbl_RealPortfolios AS PP on  PS.RealPortID = PP.RealPortID                                 ");
        //    sb.AppendLine("                 INNER JOIN tbl_Securities as S  ON PS.idSecurity = S.idSecurity                                          ");
        //    sb.AppendLine("                 Left outer join tbl_Prices AS Pr ON PS.idSecurity = Pr.idSecurity                                        ");
        //    sb.AppendLine("                 WHERE  PS.flWeight > 0 and Pr.dDate = '{0}'                                                              ");
        //    sb.AppendLine("                 GROUP BY  PS.RealPortID                                                                                  ");
        //    sb.AppendLine("                 )   PGR on PGR.RealPortID = P.RealPortID                                                                 ");
        //    sb.AppendLine("ORDER BY P.RealPortID desc                                                                                                ");

        //    string sqlStatement = string.Format(sb.ToString(), DateTime.Today.AddDays(-1).ToString("yyyy/MM/dd"));

        //    return sqlStatement;
        //}//getAllNotImportedPortfolios

        //public static String getRealPortFinalDetailsSecsByPortIdSQL(string iPortId, string calcCurrency)
        //{ // Retrieves all securities belonging to a certain portfolio
        //    string stdFldName = "stdYield";
        //    if (calcCurrency == "ILS")
        //        stdFldName = "stdYieldNIS";

        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("SELECT  PS.idSecurity , PS.flWeight , PS.flQuantity                                                                                           ");
        //    // Replacing next 2 lines, because tbl_Prices might be not there for the securities, so we are bringing from the Server last prices and merging with this table
        //    //sb.AppendLine("      , (Case when Pr.{2} is NULL then 0 else Pr.{2} end) / (Case when S.FAC is NULL then 1 else S.FAC end) as fClose                         ");
        //    //sb.AppendLine("      , PS.flQuantity * (Case when Pr.{2} is NULL then 0 else Pr.{2} / {3} end) / (Case when S.FAC is NULL then 1 else S.FAC end) as SecValue "); 
        //    sb.AppendLine("      , cast(0 as float) as LastPrice                                                                                                         ");
        //    sb.AppendLine("      , cast(0 as float) as SecValue                                                                                                          ");
        //    sb.AppendLine("      , S.dtPriceEnd as LastTradingDate                                                                                                       ");
        //    sb.AppendLine("      , S.strSymbol  , S.strName , S.HebName                                                                                                  ");
        //    sb.AppendLine("      , S.idMarket, S.idSector, S.idSecurityType                                                                                              ");
        //    sb.AppendLine("      , S.idCurrency                                                                                                                          ");
        //    sb.AppendLine("      , S.{2} as StdDev                                                                                                                       ");
        //    sb.AppendLine("FROM tbl_RealPortfolioSecurities as PS                                                                                                        ");
        //    sb.AppendLine("INNER JOIN tbl_Securities as S  ON PS.idSecurity = S.idSecurity                                                                               ");
        //    sb.AppendLine("Left outer join tbl_Prices AS Pr ON PS.idSecurity = Pr.idSecurity                                                                             ");
        //    sb.AppendLine("WHERE PS.RealPortID = {0} and PS.flWeight > 0                                                                                                 ");
        //    sb.AppendLine("      and Pr.dDate = '{1}'                                                                                                                    ");

        //    string sqlStatement = string.Format(sb.ToString(), iPortId, DateTime.Today.AddDays(-1).ToString("yyyy/MM/dd"), stdFldName); 

        //    return sqlStatement;
        //}//getSecuritiesByBaseSQL

        //public static String getRealPortfoliosSectorStats()
        //{ // Retrieves the statistics for the sectors of all Real-Portfolios
        //    StringBuilder sb = new StringBuilder();

        //    sb.AppendLine("SELECT        tbl_RealPortfolios.RealPortID, tbl_RealPortfolios.RealPortName, tblSel_Sectors.idSector, tblSel_Sectors.strName AS SectorName, ");
        //    sb.AppendLine(" COUNT(tbl_RealPortfolioSecurities.idSecurity) AS NumSecs                                                                                    ");
        //    sb.AppendLine(" FROM            tbl_RealPortfolios INNER JOIN                                                                                               ");
        //    sb.AppendLine("tbl_RealPortfolioSecurities ON tbl_RealPortfolios.RealPortID = tbl_RealPortfolioSecurities.RealPortID INNER JOIN                             ");
        //    sb.AppendLine("tbl_Securities ON tbl_RealPortfolioSecurities.idSecurity = tbl_Securities.idSecurity INNER JOIN                                              ");
        //    sb.AppendLine("tblSel_Sectors ON tbl_Securities.idSector = tblSel_Sectors.idSector                                                                          ");
        //    sb.AppendLine("GROUP BY tbl_RealPortfolios.RealPortID, tbl_RealPortfolios.RealPortName, tblSel_Sectors.idSector, tblSel_Sectors.strName                     ");

        //    string sqlStatement = string.Format(sb.ToString(), DateTime.Today.AddDays(-1).ToString("yyyy/MM/dd"));

        //    return sqlStatement;
        //}//getRealPortfoliosSectorStats

        #endregion Real Portfolio

        #region constraint

        public static String insertNewConstraint(cConstraint cCurCons, int pordId)
        {
            string min = (cCurCons.Minimum == double.MinValue) ? "NULL" : ((float)cCurCons.Minimum).ToString();
            string max = (cCurCons.Maximum == double.MaxValue) ? "NULL" : ((float)cCurCons.Maximum).ToString();
            string equal = (cCurCons.Equality == double.MaxValue) ? "NULL" : ((float)cCurCons.Equality).ToString();

            int iConstraintType = Convert.ToInt32(cCurCons.ConstraintType);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INSERT INTO tbl_Constraints (idPortFolio, txtDisplay, iConstraintType, dEquality, dMin, dMax)       ");
            sb.AppendLine("                    VALUES ( {0} ,                                                                  ");
            sb.AppendLine("                            '{1}',                                                                  ");
            sb.AppendLine("                             {2} ,                                                                  ");
            sb.AppendLine("                             {3} ,                                                                  ");
            sb.AppendLine("                             {4} ,                                                                  ");
            sb.AppendLine("                             {5} )                                                                  ");

            string sqlStatement = string.Format(sb.ToString(), pordId.ToString(),
                                                               cCurCons.DisplayText,
                                                               iConstraintType.ToString(),
                                                               equal,
                                                               min,
                                                               max);
            return sqlStatement;
        }//insertNewConstraint

        public static String insertNewConstraintItem(int idCons, String idItem)
        {
            String sqlStatement = string.Format("INSERT INTO tbl_ConstraintItems (idConstraint, idItem) VALUES ({0}, '{1}');", idCons.ToString(), idItem.ToString());
            return sqlStatement;
        }//insertNewConstraintItem

        #endregion constraint

        #region General

        public static String insertCategoryItemToDbSQL(String strTblName, String strVal)
        { return string.Format("INSERT INTO {0} (strName) VALUES ('{1}');", strTblName, strVal); }//insertCategoryItemToDbSQL

        public static String getTblItemsSQL(String strTblName)
        { return string.Format("SELECT * FROM {0}", strTblName); }//getTblItemsSQL

        public static String getTblValsByStrSelectConditionSQL(String strTblName, String strSelect)
        { return string.Format("SELECT * FROM {0} WHERE {1} ;", strTblName, strSelect); }//getTblValsByStrSelectConditionSQL

        public static String getTblValsByStrConditionSQL(String strTblName, String strFldName, String strVal)
        { return string.Format("SELECT * FROM {0} WHERE {1} = '{2}';", strTblName, strFldName, strVal); }//getTblValsByStrConditionSQL

        public static String getTblValsByIntConditionSQL(String strTblName, String strFldName, long iVal)
        { return string.Format("SELECT * FROM {0} WHERE {1} = {2};", strTblName, strFldName, iVal.ToString()); }//getTblValsByIntConditionSQL

        public static String removeDataFromTblByIdIntSQL(String strTblName, String strFldName, long iId)
        { return string.Format("DELETE FROM {0} WHERE {1} = {2} ;", strTblName, strFldName, iId.ToString()); }//removeDataFromTblByIdIntSQL

        public static String removeDataFromTblByIdStrSQL(String strTblName, String strFldName, String strIdSec)
        { return string.Format("DELETE FROM {0} WHERE {1} = '{2}';", strTblName, strFldName, strIdSec); }//removeSecFromTblByID

        public static String removeDataBySecIdAndPortIdSQL(String iSecId, int iPortId)
        { return string.Format("DELETE FROM tbl_PortfolioSecurities WHERE idSecurity = '{0}' AND idPortfolio = {1};", iSecId.ToString(), iPortId.ToString()); }//removeDataBySecIdAndBaseIdSQL

        public static String getStockMarketTable()
        { return "Select * From tblSel_StockMarkets where idStockMarket in (1,3,4,5)	order by strName"; }//getStockMarketTable

        #endregion General

        #region Edit portfolio

        public static String addSecurityToPersonalPool(string sSecurityID)
        { // Inserts 1 record to tbl_PersonalPool

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("INSERT INTO [tbl_PersonalPool]       ");
            sb.AppendLine("            ([idSecurity])           ");
            sb.AppendLine("VALUES      ('{0}')                  ");

            string sqlStatement = string.Format(sb.ToString(), sSecurityID);

            return sqlStatement;

        }//addSecurityToPersonalPool

        public static String deleteSecuritiesFromPersonalPool(string listSecurityID)
        {   // Deletes records from table tbl_PersonalPool,
            // list of ids - '1', '2', '3',...

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("DELETE FROM [tbl_PersonalPool]       ");
            sb.AppendLine("WHERE  idSecurity in ({0})           ");

            string sqlStatement = string.Format(sb.ToString(), listSecurityID);

            return sqlStatement;

        }//deleteSecuritiesFromPersonalPool

        #endregion

        #region Lookup combo boxes

        public static String getCurrencyDataTable()
        {   // Get list of currency for lookup combo box

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT [idCurrency],[strName],[strDesc]      ");
            sb.AppendLine("FROM [tblSel_Currency]                       ");
            sb.AppendLine("WHERE idCurrency in ('ILS','9001')          ");

            string sqlStatement = sb.ToString();

            return sqlStatement;

        }//getCurrencyDataTable


        #endregion

        #region Benchmarks

	    public static String getBenchmarkSecurities(string BenchmarksIDlist)
        { // Retrieves Benchmarks securities from local DB

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT S.*                                   ");
            sb.AppendLine("FROM   v_Securities S                      ");
            sb.AppendLine("WHERE S.idSecurity in ({0})                  ");

            string sqlStatement = string.Format(sb.ToString(), BenchmarksIDlist);

            return sqlStatement;
        }//getRealPortfoliosSectorStats


	    public static String getBenchmarkPricesInDateRange(string BenchmarksIDlist, DateTime startDate, DateTime endDate)
        { // Retrieves Benchmarks prices in Date range from local DB

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT   idSecurity                                         ");
            sb.AppendLine("       , dDate                                              ");
            sb.AppendLine("       , COALESCE(FAC, 1.0) as FAC                          ");
            sb.AppendLine("       , 1 as dFacAcc                                       "); // this column is needed, because same code is excuted for BM covariance calculations
            sb.AppendLine("       , fClose                                             ");
            sb.AppendLine("       , fNISClose                                          ");
            sb.AppendLine("       , {3} as {4}                                         "); // it is possible because for Benchmarks FAC = 1
            sb.AppendLine("       , isHoliday                                          ");
            sb.AppendLine("FROM v_Prices                                             ");
            sb.AppendLine("WHERE idSecurity in ({0})                                   ");
            sb.AppendLine("      and dDate between '{1}' and '{2}'                     ");
            sb.AppendLine("ORDER BY idSecurity, dDate desc                             ");

            string sqlStatement = string.Format(sb.ToString(), BenchmarksIDlist
                                                             , cGeneralFunctions.getDateFormatStrForSQL(startDate, "-")
                                                             , cGeneralFunctions.getDateFormatStrForSQL(endDate, "-")
                                                             , cProperties.ClosePriceFld
                                                             , cProperties.AdjPriceFld);

            return sqlStatement;
        }//getRealPortfoliosSectorStats

        #endregion

        #endregion SQL Statement Methods

    }//of class
}
