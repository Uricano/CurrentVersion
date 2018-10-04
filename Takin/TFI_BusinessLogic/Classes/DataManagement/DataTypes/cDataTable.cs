using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
//using System.Diagnostics;

// Used namespaces
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.General;

namespace Cherries.TFI.BusinessLogic.DataManagement.DataTypes
{
    public class cDataTable : IDisposable
    { // Handles Datatable operations

        #region Data members

        // Main variables 
        private DataTable m_dtMain; // Main Datatable
        private cSecurities m_objSecCollection; // Collection of active securities
        private cErrorHandler m_objErrorHandler; // Error handler
        private bool isDisposed = false; // indicates if Dispose has already been called

        // Data variables
        private List<double> m_colFinalVals = new List<double>(); // sorted array
        private double m_dMinVal = double.MaxValue, m_dMaxVal = double.MinValue; // Min and max values

        private double m_Middle = 0;

        // General variables
        private String m_strSecurityFldName; // Name of securities field in main datatable
        private String m_strValueFldName; // Name of value field (for preparation of work matrix)

        #endregion Data members

        #region Constructors, Initialization & Destructor

        public cDataTable(DataTable myDataTable, cSecurities mySecurities, cErrorHandler cErrHandler, String strSecFld, String strValueFld)
        { // This constructor is for the new table format
            // Init variables
            m_dtMain = myDataTable;
            m_objErrorHandler = cErrHandler;
            m_objSecCollection = mySecurities;
            m_strSecurityFldName = strSecFld;
            m_strValueFldName = strValueFld;

            try
            {
                setMinMaxVals();

            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
            //if (m_dtMain.Rows.Count != 0) m_dWorkMatrix = getWorkMatrixCrossTbl(); // set working matrix
        }// constructor

        ~cDataTable()
        { Dispose(false); }//destructor

        protected void Dispose(bool disposing)
        { // clearing class variables
            if (disposing)
            { //clean up managed resources
                m_objSecCollection = null;
                m_objErrorHandler = null;
                m_colFinalVals.Clear();
                if (m_dtMain != null) m_dtMain.Dispose();
            }
            isDisposed = true;
        }//Dispose

        public void Dispose()
        { // indicates it was NOT called by the Garbage collector
            Dispose(true);
            GC.SuppressFinalize(this); // no need to do anything, stop the finalizer from being called
        }//Dispose

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region DataTable presentation

        public static DataTable getDiagonalDataTable(DataTable dtOrigTbl)
        { // Transforms normal matrix table to a diagonal matrix (in covariance and correlation matrices)
            // Table structure: strSec1, strSec2, dblVal
            dtOrigTbl.DefaultView.Sort = "strSec1";
            String strSec1, strSec2;
            double dVal;
            DataTable dtFinalTbl = dtOrigTbl.Clone(); // Copy data structure

            int i_Rows = Convert.ToInt32(System.Math.Sqrt(dtOrigTbl.Rows.Count));// grid Max rows
            int i_CalcedIndex = dtOrigTbl.Rows.Count - 1;
            for (int i_RowCount = dtOrigTbl.Rows.Count; 0 < i_RowCount; i_RowCount = i_RowCount - i_Rows)//Covar Correl grid rows
                for (int i_SqrRowCount = i_Rows; i_SqrRowCount > 0; i_SqrRowCount--)//Covar Correl grid columns
                {
                    i_CalcedIndex = i_RowCount - (i_Rows - i_SqrRowCount) - 1;

                    strSec1 = dtOrigTbl.DefaultView.Table.Rows[i_CalcedIndex]["strSec1"].ToString();
                    strSec2 = dtOrigTbl.DefaultView.Table.Rows[i_CalcedIndex]["strSec2"].ToString();
                    dVal = (double)dtOrigTbl.DefaultView.Table.Rows[i_CalcedIndex]["dblVal"];

                    dtFinalTbl.Rows.Add(strSec1, strSec2, dVal);

                    if (String.Compare(strSec1, strSec2) == 0) break;
                }
            return dtFinalTbl;
        }//getDiagonalDataTable

        private void setMinMaxVals()
        { // Sets the minimum and maximum values for the datatable
            m_dMinVal = double.MaxValue;
            m_dMaxVal = double.MinValue;
            foreach (DataRow dr in m_dtMain.Rows)
            {
                double dCurrVal = dr.Field<double>(m_strValueFldName);
                m_dMinVal = Math.Min(m_dMinVal, dCurrVal);
                m_dMaxVal = Math.Max(m_dMaxVal, dCurrVal);
            }
        }//setMinMaxVals

        #endregion DataTable presentation

        #endregion Methods

        #region Properties

        public DataTable DataTbl
        { get { return m_dtMain; } } // Main datatable

        public double Minimum
        { get { return m_dMinVal; } }//Minimum

        public double Middle { get { return m_Middle; } } 

        public double Maximum
        { get { return m_dMaxVal; } }//Maximum

        public List<double> SortedArray
        { get { return m_colFinalVals; } }//SortedArray

        #endregion

    }//of class
}