using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;


//Used namespaces
using Cherries.TFI.BusinessLogic.General;
using System.Configuration;
using System.Collections;

namespace Cherries.TFI.BusinessLogic.DataManagement
{
    public class cDbOleConnection
    {

        #region Data Members

        // Main variables
        //private SqlConnection m_objSqlConn; // Db connection
        private SqlConnection m_objSqlConn; // Db connection
        private cErrorHandler m_objErrorHandler; // Error handler class
        private String m_strDbFileName; // Database file name

        private static String m_strDbName = "dbCherries.sdf"; // Database name
        private static String m_strDbConnString = "";
        private static SqlConnection m_SqlCeConnection = null;

        private cDBWorker _DBWorker;

        // Constants
        //private const string m_cnstServerConnStr = "Server=COMP1-PC\\SQL2008;Database=TakinDev;User ID=uriel;Password=1234;Trusted_Connection=False;"; // Connection string for SQL-Server database
        private static readonly String m_cnstPortTblName = "tbl_Portfolios";
        private static readonly String m_cnstPortSecTblName = "tbl_PortfolioSecurities";
        private static readonly String m_cnstSecsTblName = "tbl_Securities";
        private static readonly String m_cnstSecPricesTbl = "tbl_Prices";
        private static readonly String m_cnstSecsBaseTbl = "tbl_Bases";
        private static readonly String m_cnstCurrencyTbl = "tblSel_Currency";
        private static readonly String m_cnstIndustriesTbl = "tblSel_Industries";
        private static readonly String m_cnstSectorsTbl = "v_tbSel_Sectors";
        private static readonly String m_cnstSecTypeTbl = "v_tbSel_SecurityTypes";
        private static readonly String m_cnstMarketsTbl = "v_tbSel_StockMarkets";
        private static readonly String m_cnstConstraintsTbl = "tbl_Constraints";
        private static readonly String m_cnstConstraintItemsTbl = "tbl_ConstraintItems";
        private static readonly String m_cnstGroupsTbl = "tbl_SecGroups";
        private static readonly String m_cnstGroupItemsTbl = "tbl_SecGroupItems";
        private static readonly String m_cnstInvestPortTbl = "tbl_InvestPortfolios";
        private static readonly String m_cnstInvestmentTbl = "tbl_Investments";
        private static readonly String m_cnstInvestActionsTbl = "tbl_InvestActions";

        private static readonly String m_cnstPriceTmpTbl = "tblTmp_Prices";
        private static readonly String m_cnstPricesFullTmpTbl = "tblTmp_PricesFull";

        #endregion Data Members

        #region Constructors, Initialization & Destructors

        public cDbOleConnection(cErrorHandler cErrHandler)
        {
            m_objErrorHandler = cErrHandler;
            try
            {
                //m_strDbFileName = getCroppedPath(cProperties.DataFolder + "\\Database\\" + m_strDbName);
                m_strDbFileName = cProperties.DataFolder + "\\Database\\" + m_strDbName;
                setDbConnection();
                _DBWorker = new cDBWorker(m_objSqlConn);
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
                
            }
        }// of constructor

        #endregion Constructors, Initialization & Destructors

        #region Methods

        #region Connection

        private void setDbConnection()
        { // Sets the connection with the proper permissions
            m_strDbConnString = ConfigurationManager.ConnectionStrings["ConnStr"].ConnectionString; //"Data Source=" + m_strDbFileName + "; password=Tak_1234; Max Database Size=1024;";
            m_objSqlConn = new SqlConnection(m_strDbConnString);
            m_SqlCeConnection = m_objSqlConn; // new
            m_objSqlConn.Open();
        }//setDbConnection

        private String getCroppedPath(String strMainName)
        { return strMainName.Replace("\\bin\\Debug", ""); }//getCroppedPath

        #endregion Connection

        #region Find Data

        public static Boolean isTableValueExists(String strTbl, String strConditionFld, String strConditionVal, SqlConnection sqlConnection)
        { // Verifies the table has a value available after a given condition
            SqlCommand sqlNewCommand = new SqlCommand(cSqlStatements.getTblValsByStrConditionSQL(strTbl, strConditionFld, strConditionVal), sqlConnection);
            SqlDataReader    sqlNewReader = sqlNewCommand.ExecuteReader();
            if (sqlNewReader.Read()) { sqlNewReader.Dispose(); return true; } // Found value
            sqlNewReader.Dispose();
            return false;
        }//isTableValueExists

        public static String getTblValueFromCondition(String strTbl, String strConditionFld, String strConditionVal, String strReturnFld, SqlConnection sqlConnection)
        { // Retrieves the specified table value (based on the given condition)
            SqlCommand sqlNewCommand = new SqlCommand(cSqlStatements.getTblValsByStrConditionSQL(strTbl, strConditionFld, strConditionVal), sqlConnection);
            SqlDataReader sqlNewReader = sqlNewCommand.ExecuteReader();
            if (sqlNewReader.Read()) 
            {
                return sqlNewReader[strReturnFld].ToString();
                //sqlNewReader.Dispose(); 
            } // Found value
            sqlNewReader.Dispose();
            return "-1";
        }//getTblValue

        public static String getTblValueWithSQLstmt(String strTbl, String strSelect, String strReturnFld, SqlConnection sqlConnection)
        { // Retrieves the specified table value (based on the given condition)
            SqlCommand sqlNewCommand = new SqlCommand(cSqlStatements.getTblValsByStrSelectConditionSQL(strTbl, strSelect), sqlConnection);
            SqlDataReader sqlNewReader = sqlNewCommand.ExecuteReader();
            if (sqlNewReader.Read())
            {
                return sqlNewReader[strReturnFld].ToString();
                //sqlNewReader.Dispose(); 
            } // Found value
            sqlNewReader.Dispose();
            return "-1";
        }//getTblValue

        public static Boolean isTableExists(String strTblName, SqlConnection sqlConnection)
        { // Checks whether a specified table exists in our database
            string strSql = @"SELECT COUNT(*) FROM " + strTblName;
            try
            {
                using (SqlCommand cmd = new SqlCommand(strSql, sqlConnection))
                {
                    cmd.ExecuteScalar();
                    return true;
                }
            } catch {
                return false;
            } 
        }//isTableExists

        //public String getSecIdBySymbol(String strVal)
        //{ // Retrieves the security's ID by its symbol
        //    SqlCeDataReader sqlNewReader = null;
        //    try
        //    {
        //        SqlCeCommand sqlNewCommand = new SqlCeCommand(cSqlStatements.getTblValsByStrConditionSQL(cDbOleConnection.TblSecs, "strSymbol", strVal), m_objSqlConn);
        //        sqlNewReader = sqlNewCommand.ExecuteReader();
        //        if (sqlNewReader.Read())
        //            return sqlNewReader["idSecurity"].ToString();
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    } finally {
        //        if (sqlNewReader != null) sqlNewReader.Dispose();
        //    }
        //    return "-1";
        //}//getSecIdBySymbol

        #endregion Find Data

        #region DataTable handler

        public static DataTable FillDataTable(string SqlSelectCmd, SqlConnection sqlConnection)
        { // Fills a Datatable with the data found in the connection
            SqlDataAdapter myDataAdapter = new SqlDataAdapter();
            DataTable dtMain = new DataTable();

            try
            {
                myDataAdapter.SelectCommand = new SqlCommand(SqlSelectCmd, sqlConnection);

                myDataAdapter.FillSchema(dtMain, SchemaType.Mapped);
                myDataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                myDataAdapter.Fill(dtMain);
            } catch (Exception ex) { throw ex; }
            return dtMain;
        }//FillDataTable

        public static DataTable FillDataTable(string sp, SqlConnection sqlConnection, ArrayList prmArrParms)
        {
            SqlDataAdapter myDataAdapter = new SqlDataAdapter();
            DataTable dtMain = new DataTable();
            
           
            try
            {
                myDataAdapter.SelectCommand = new SqlCommand(sp, sqlConnection);
                myDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter CurrParm_loopVariable in prmArrParms)
                {
                    SqlParameter p = CurrParm_loopVariable;
                    myDataAdapter.SelectCommand.Parameters.Add(p);
                }
                myDataAdapter.SelectCommand.CommandTimeout = int.MaxValue;
                myDataAdapter.FillSchema(dtMain, SchemaType.Mapped);
                myDataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                myDataAdapter.Fill(dtMain);
            }
            catch (Exception ex) { throw ex; }
            return dtMain;
        }//FillDataTable
    

        public static DataTable updateDataTable(string SqlSelectCmd, DataTable dtData, SqlConnection sqlConnection)
        { // Fills a Datatable with the data found in the connection
            SqlDataAdapter myDataAdapter = new SqlDataAdapter();
            DataTable dtMain = new DataTable();

            try
            {
                myDataAdapter.SelectCommand = new SqlCommand(SqlSelectCmd, sqlConnection);
                myDataAdapter.FillSchema(dtMain, SchemaType.Mapped);
                //myDataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                SqlCommandBuilder cb = new SqlCommandBuilder(myDataAdapter);

                myDataAdapter.UpdateCommand = cb.GetUpdateCommand();
                myDataAdapter.Update(dtData);
            }
            catch (Exception ex) { throw ex; }
            return dtMain;
        }//FillDataTable

        public static void executeSqlSatatement(string SqlSelectCmd, SqlConnection sqlConnection)
        { // Executes an SQL command
            SqlCommand sqlCommand = new SqlCommand(SqlSelectCmd, sqlConnection);
            sqlCommand.ExecuteNonQuery();
            sqlCommand.Dispose();
        }//executeSqlSatatement
        public static object executeSqlScalar(string SqlSelectCmd, SqlConnection sqlConnection)
        { // Executes an SQL command and returns the scalar
            SqlCommand sqlCommand = new SqlCommand(SqlSelectCmd, sqlConnection);
            try
            {
                sqlCommand.ExecuteNonQuery();

                sqlCommand.CommandText = "SELECT @@IDENTITY";
                object objScalar = sqlCommand.ExecuteScalar();
                if (objScalar == DBNull.Value) return -1;
                return Convert.ToInt32(objScalar);
            }
            catch (Exception ex) { }
            finally { sqlCommand.Dispose(); }
            return null;
        }//executeSqlScalar
        public static object executeSqlScalarNoIDENTITY(string SqlSelectCmd, SqlConnection sqlConnection)
        { // Executes an SQL command and returns the scalar
            SqlCommand sqlCommand = new SqlCommand(SqlSelectCmd, sqlConnection);
            try
            {
                sqlCommand.ExecuteNonQuery();

                object objScalar = sqlCommand.ExecuteScalar();
                if (objScalar == DBNull.Value) return -1;
                return Convert.ToInt32(objScalar);
            }
            catch (Exception ex) { }
            finally { sqlCommand.Dispose(); }
            return null;
        }//executeSqlScalarNoIDENTITY

        public static void writePricesWithBulk(DataTable dtMain,SqlConnection cConn, String strDestTbl )
        { // Copies prices to database
            if (dtMain.Rows.Count == 0) return;

            SqlBulkCopyOptions options = new SqlBulkCopyOptions();
            options = options |= SqlBulkCopyOptions.KeepNulls;

            using (SqlBulkCopy bc = new SqlBulkCopy(cConn.ConnectionString, options))
            {
                bc.DestinationTableName = strDestTbl;
                bc.WriteToServer(dtMain);
            }
        }//writePricesWithBulk

        public static DataTable getDataTableEditable(DataTable dtMain, Boolean readonlyVal)
        { // Transforms a datatable to either editable or readonly
            for (int iCols = 0; iCols < dtMain.Columns.Count; iCols++)
                dtMain.Columns[iCols].ReadOnly = readonlyVal;
            return dtMain;
        }//getDataTableEditable

        #endregion DataTable handler

        #region Import related methods

        public List<DataRow> setLocalDbSecuritiesCollection(ref List<DataRow> colSelectedSecs)
        { // Sets & reads the securities found on local DB
            // Remove securities from collection of SecsForImport (if exists in LocalDB)
            List<DataRow> lstRowsToDelete = new List<DataRow>();
            List<DataRow> lstLocalSecs = new List<DataRow>();
            try
            {
                DataTable dtLocalDbSecs = cDbOleConnection.FillDataTable(cSqlStatements.getSecuritiesList(colSelectedSecs), m_SqlCeConnection);

                lstLocalSecs.Clear();
                foreach (DataRow dr in colSelectedSecs)
                { // Goes through list of selected securities
                    DataRow[] arrFound = dtLocalDbSecs.Select("idSecurity = '" + dr["idSecurity"].ToString() + "'");
                    if (arrFound.Length > 0)
                    { // Found on Local DB
                        lstRowsToDelete.Add(dr);
                        lstLocalSecs.Add(arrFound[0]);
                    }
                }//main for

                for (int iSecs = 0; iSecs < lstRowsToDelete.Count; iSecs++)
                    colSelectedSecs.Remove(lstRowsToDelete[iSecs]); // Remove securities from server import
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
            return lstLocalSecs;
        }//setLocalDbSecuritiesCollection

        #endregion Import related methods

        #endregion Methods

        #region Properties

        public static String DbName
        { get { return m_strDbName; } }//DbName

        public static String ConnectionString
        { get { return m_strDbConnString; } }

        public static SqlConnection sqlCeConnection
        { get { return m_SqlCeConnection; } }

        public SqlConnection dbConnection
        { get { return m_objSqlConn; } }//dbConnection property

        public cDBWorker dbWorker { get { return _DBWorker; } }

        public static String TblPort
        { get { return m_cnstPortTblName; } }//TblPort

        public static String TblSecs
        { get { return m_cnstSecsTblName; } }//TblSecs

        public static String TblPortSecs
        { get { return m_cnstPortSecTblName; } }//TblPortSecs

        public static String TblPrices
        { get { return m_cnstSecPricesTbl; } }//TblRates

        public static String TblBases
        { get { return m_cnstSecsBaseTbl; } }//TblBases

        public static String TblCurrency
        { get { return m_cnstCurrencyTbl; } }//TblCurrency

        public static String TblSecType
        { get { return m_cnstSecTypeTbl; } }//TblSecType

        public static String TblIndustries
        { get { return m_cnstIndustriesTbl; } }//TblIndustries

        public static String TblSectors
        { get { return m_cnstSectorsTbl; } }//TblSectors

        public static String TblMarkets
        { get { return m_cnstMarketsTbl; } }//TblMarkets

        public static String TblConstraints
        { get { return m_cnstConstraintsTbl; } }//TblConstraints

        public static String TblConstraintItems
        { get { return m_cnstConstraintItemsTbl; } }//TblConstraintItems

        public static String TblGroups
        { get { return m_cnstGroupsTbl; } }//TblGroups

        public static String TblGroupItems
        { get { return m_cnstGroupItemsTbl; } }//TblGroupItems

        public static String TblInvestPortfolio
        { get { return m_cnstInvestPortTbl; } }//TblInvestPortfolio
        
        public static String TblInvestments
        { get { return m_cnstInvestmentTbl; } }//TblInvestments

        public static String TblInvestActions
        { get { return m_cnstInvestActionsTbl; } }//TblInvestActions

        public static String TblTmpPrices
        { get { return m_cnstPriceTmpTbl; } }//TblTmpPrices

        public static String TblTmpFullPrices
        { get { return m_cnstPricesFullTmpTbl; } }//TblTmpFullPrices

        #endregion Properties

    } // of class
}
