using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cherries.TFI.BusinessLogic.General;
using System.Data;
using Cherries.TFI.BusinessLogic.DataManagement;
using TFI.BusinessLogic.Enums;

namespace Cherries.TFI.BusinessLogic.Tools.Settings
{
    public class cParameters
    {
       #region Data members

        // Main variables
        private cErrorHandler m_objErrorHandler;            // Error handler class

        // Data variables
        private static DataTable m_dtParameters = null;     // tbl_Parameters dataTable

        #endregion Data members

        #region Constructors, Initialization & Destructor

        public cParameters(cErrorHandler cErrors)
        {
            m_objErrorHandler = cErrors;
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Methods

        public void initParams()
        { // Reads data from local datatable tbl_Parameters into the variable
            m_dtParameters = cDbOleConnection.FillDataTable("SELECT  Name, Type, Value FROM tbl_Parameters", cDbOleConnection.sqlCeConnection);
        }//initParams

        public string paramByName(String strName)
        { // Finds and returns parameter value by specified name
          // In case it is not found "" is returned (not null)
            try
            {
                if (m_dtParameters == null)
                    initParams();

                DataRow dr = m_dtParameters.Rows.Find(strName);

                if (dr == null)
                    return "";
                else
                    return dr["Value"].ToString();
            
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
                throw;
            }
        }//paramByName

        public void updateParamByValue(string strName, string strValue)
        { // Updates parameter by name
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("UPDATE tbl_Parameters              ");
            sb.AppendLine("SET    Value = '{0}'               ");
            sb.AppendLine("WHERE  Name  = '{1}'               ");

            cDbOleConnection.executeSqlSatatement(string.Format(sb.ToString(), strValue, strName), cDbOleConnection.sqlCeConnection);
        }//updateParamByDate

        public void setAppDefaultValues()
        { // Sets the default values read from the settings file to the current application thread
            try
            {
                for (int iProps = 0; iProps < m_dtParameters.Rows.Count; iProps++)
                    setCPropertiesVal(m_dtParameters.Rows[iProps]);
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setAppDefaultValues

        private void setCPropertiesVal(DataRow cItem)
        { // Sets the value of a certain item to cProperties file
            String strName = cItem["Name"].ToString();
            String strVal = cItem["Value"].ToString();
            switch (strName)
            {
                case "SecDisplay": cProperties.DisplayOfSecurity = cEnumHandler.getSecurityDisplayVal(strVal); break;
                case "CurrencyId": cProperties.CurrencyId = strVal; break;
                case "maxRisk": cProperties.maxRiskVal = Convert.ToDouble(strVal); break;
                case "minDataMonths": cProperties.MinMonthsData = Convert.ToInt32(strVal); break;
            }
        }//setCPropertiesVal

        #endregion Methods

    }//of class
}
