using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Cherries.TFI.BusinessLogic.DataManagement;

// DELETE NEXT 2 LINES WHEN MERGING
using System.Diagnostics;

namespace Cherries.TFI.BusinessLogic.General
{
    public class cHelpManagement
    {
        #region Data Members

        private string m_TableName = "tbl_HelpDef";
        private DataTable m_tblHelpDef = null;         // tbl_HelpDef table of LocalDB
        
        #endregion Data Members

        #region Methods

        
        private DataTable GetTableData(string tableName)
        { // Retrieves tbl_HelpDef (local DB - dbCherries)

            return cDbOleConnection.FillDataTable(cSqlStatements.getTblItemsSQL(tableName), cDbOleConnection.sqlCeConnection);

        }//GetTableData

        private int GetHelpFilePageNo(int windowID)
        {// Searches local Datatable tbl_HelpDef for the correct page by given Window ID 
         // (set in the TAG property of help ('?') button of each form or user control)

            if (m_tblHelpDef == null)
                m_tblHelpDef = GetTableData(m_TableName);

            DataRow[] arrDR = m_tblHelpDef.Select(string.Format("WindowID = {0}", windowID));

            if (arrDR.Length > 0)
                return Convert.ToInt32(arrDR[0]["PageNo"]);

            return -1;
        }//GetHelpFilePageNo

        #endregion

    }
}
