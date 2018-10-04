using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;


// Used namespaces
using Cherries.TFI.BusinessLogic.Collections;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.DataManagement;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.Tools;
using TFI.BusinessLogic.Enums;
using Ness.DataAccess.Repository;
using TFI.BusinessLogic.Interfaces;
using TFI.BusinessLogic.Bootstraper;

namespace Cherries.TFI.BusinessLogic.Constraints
{
    public class cConstHandler : IDisposable
    {

        #region Data Members

        // Main variables
       
        private IPortfolioBL m_objPortfolio; // Portfolio class pointer
        private IErrorHandler m_objErrorHandler; // Error handler
        private cConstraints m_colConstraints;// = new cConstraints(); // Constraints list
        private ICollectionsHandler m_objColHandler; // Collections handler
        private bool isDisposed = false; // Dispose indicator

        // General variables
        private cConstraints m_colSingleSecs = new cConstraints(); // Single security constraints
        private cConstraints m_colEquality = new cConstraints(); // Equality constraints
        private cConstraints m_colRange = new cConstraints(); // Range constraints
        private cConstraints m_colDefaultEquality = new cConstraints(); // Default equality constraints
        private cConstraints m_colDefaultRange = new cConstraints(); // Default range constraints
        private DataTable m_dtMainConsts; // Constraints DataTable
        private double[] m_arrSecsUpperBound, m_arrSecsLowerBound; // Securities lists of upper and lower bounds
        private int[] m_arrConstItemsState; // Array containing the securities participating in the current constraint (by 0/1)
        private const Boolean m_isSumTo100 = true; // Securities' weights add up to exactly 100%
        private const Boolean m_is100OrLess = false; // Securities' weights add up to 100 or less

        // Constraint Strings
        private List<List<double>> m_doubleEqualitySecs;      // --c1--  List containing securities matrix for equality constraints
        private List<double> m_doubleEqualityVals;            // --b1--  List containing values of equality constraints
        private int m_intEqualityNum;                         // --me--  String containing the number of equality constraints 
        private List<List<double>> m_doubleRangeSecs;         // --c2--  List containing securities matrix for range constraints
        private List<double> m_doubleRangeVals;               // --b2--  List containing values of range constraints

        // Empty constraint strings
        private double[] m_arrEmptySecsUpperBound;            // Securities lists empty upper bounds
        private double[] m_arrEmptySecsLowerBound;            // Securities lists empty lower bounds
        private List<List<double>> m_emptyDoubleEqualitySecs; // --c1--  List containing no equality constraints (except for the defaults)
        private List<double> m_emptyDoubleEqualityVals;       // --b1--  List containing values of equality constraints
        private List<List<double>> m_emptyDoubleRangeSecs;    // --c2--  List containing securities matrix for range constraints
        private List<double> m_emptyDoubleRangeVals;          // --b2--  List containing values of range constraints
        private IRepository repository;
        #endregion Data Members

        #region Constructors, Initialization & Destructor

        public cConstHandler(cConstraints cConsts, IPortfolioBL cPort)
        {
            m_objPortfolio = cPort;
            m_colConstraints = cConsts;
            m_objErrorHandler = m_objPortfolio.cErrorLog;
            m_objColHandler = m_objPortfolio.ColHandler;
            repository = Resolver.Resolve<IRepository>();
            setSeperatedConstraintCols();
        }//constructor

        ~cConstHandler()
        { Dispose(false); }//destructor

        public void Dispose(bool disposing)
        { // Disposing class variables
            if (disposing)
            { // Managed code
                m_objErrorHandler = null;
                m_objColHandler = null;
                m_colConstraints.Clear();
                m_colSingleSecs.Clear();
                m_colEquality.Clear();
                m_colRange.Clear();
                m_colDefaultEquality.Clear();
                m_colDefaultRange.Clear();
            }
            isDisposed = true;
        }//Dispose

        public void Dispose()
        { // Clear from memory
            Resolver.Release(repository);
            Dispose(true);
            GC.SuppressFinalize(this);
        }//Dispose

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region Data initialization

        public void setDataTableStruct()
        { // Sets structure of dataTable
            if (m_dtMainConsts != null) m_dtMainConsts.Dispose();
            m_dtMainConsts = new DataTable("tbl_rptConstraints");
            m_dtMainConsts.Columns.Add("Items", typeof(String));
            m_dtMainConsts.Columns.Add("Vals", typeof(String));
            m_dtMainConsts.Columns.Add("Dist", typeof(String));
        }//setDataTableStruct

        public void showExistConstraint()
        { // Shows constraint in current portfolio in the gridConstraintsDisplay (Fills table)
            m_dtMainConsts.Clear();
            for (int i = 0; i < m_colConstraints.Count; i++)
                addNewConstToTable(m_colConstraints[i], m_colConstraints[i].DisplayText);
        }//showExistConstraint

        public void refreshConstList()
        { // Refreshes values of variables when a new list of constraints is inserted
            m_colSingleSecs.Clear();
            m_colEquality.Clear();
            m_colRange.Clear();
            setSeperatedConstraintCols();
        }//refreshConstList

        private void refreshClassPointers()
        { // Refreshes the pointer variables of the current class
            m_objColHandler = m_objPortfolio.ColHandler;
        }//refreshClassPointers

        private void setSeperatedConstraintCols()
        { // Seperates the main collection of constraints to 3 type of collections:
            // 1-Single securities, 2-Equality constraints, 3-Range constraints
            for (int iConst = 0; iConst < m_colConstraints.Count; iConst++)
                if (m_colConstraints[iConst].isOneSec) m_colSingleSecs.Add(m_colConstraints[iConst]);
                else if (m_colConstraints[iConst].isEquality) m_colEquality.Add(m_colConstraints[iConst]);
                else m_colRange.Add(m_colConstraints[iConst]);
        }//setSeperatedConstraintCols

        private void initSecBounds(ISecurities cSecsCol)
        { // Sets initial formation of securities' lower and upper bounds
            m_arrSecsUpperBound = new double[cSecsCol.Count];
            m_arrSecsLowerBound = new double[cSecsCol.Count];
            for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
                m_arrSecsUpperBound[iSecs] = 1;
        }//initSecBounds

        #endregion Data initialization

        #region Save / Load

        //public void saveConstraints(int idPort, cDbOleConnection oleDBConn)
        //{ // save the cunstraints to the DB
        //    deleteCurConstFromDB(idPort, oleDBConn);
        //    int idInsertConst;
        //    for (int i = 0; i < m_colConstraints.Count; i++)
        //    { // Browses constraints
        //        idInsertConst = (int)cDbOleConnection.executeSqlScalar(cSqlStatements.insertNewConstraint(m_colConstraints[i], idPort), oleDBConn.dbConnection);

        //        for (int j = 0; j < m_colConstraints[i].ItemIds.Count; j++)
        //            cDbOleConnection.executeSqlSatatement(cSqlStatements.insertNewConstraintItem(idInsertConst, m_colConstraints[i].ItemIds[j]), oleDBConn.dbConnection);
        //    }
        //}//saveConstraints

        //private void deleteCurConstFromDB(int portId, cDbOleConnection oleDBConn)
        //{//delete all constraint in current portfolio that we can save them and wont be duplication
        //    DataTable dtConsts = cDbOleConnection.FillDataTable(cSqlStatements.getTblValsByIntConditionSQL(cDbOleConnection.TblConstraints, "idPortFolio", portId), oleDBConn.dbConnection);

        //    for (int iRows = 0; iRows < dtConsts.Rows.Count; iRows++)
        //        cDbOleConnection.executeSqlSatatement("DELETE FROM tbl_ConstraintItems WHERE idConstraint = " + dtConsts.Rows[iRows]["idConstraint"].ToString() + ";", oleDBConn.dbConnection);

        //    cDbOleConnection.executeSqlSatatement(cSqlStatements.removeDataFromTblByIdIntSQL("tbl_Constraints", "idPortFolio", portId), oleDBConn.dbConnection);
        //}//deleteCurConstFromDB

        //public void updateConstraints(int portId, cDbOleConnection oleDBConn, cSecurities cSecsCol)
        //{//update the Constraints collection
        //    try
        //    {
        //        DataTable dtConsts = cDbOleConnection.FillDataTable(cSqlStatements.getTblValsByIntConditionSQL(cDbOleConnection.TblConstraints, "idPortFolio", portId), oleDBConn.dbConnection);

        //        m_colConstraints.Clear();
        //        for (int iRows = 0; iRows < dtConsts.Rows.Count; iRows++)
        //            m_colConstraints.Add(getNewConstraintFromDB(dtConsts.Rows[iRows], oleDBConn));   //add the new cConstraint to th collection

        //        setDataTableStruct();
        //        showExistConstraint();
        //        refreshConstList();
        //        setConstraintValues(cSecsCol); // Sets empty collections
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    }
        //}//updateConstraints

        //public cConstraint getNewConstraintFromDB(DataRow drNew, cDbOleConnection oleDBConn)
        //{ // Reads constraint from DB
        //    List<String> itemList = getListOfItems(Convert.ToInt32(drNew["idConstraint"]), oleDBConn); // Fills constraint items (secs)
        //    string sTxtDisplaye = Convert.ToString(drNew["txtDisplay"]);
        //    int iConstType = Convert.ToInt32(drNew["iConstraintType"]);
        //    double dEqual = getConstValue(drNew["dEquality"]);

        //    //saves all relevant details in temp variables
        //    if (dEqual != double.MaxValue)
        //        return new cConstraint(itemList, sTxtDisplaye, getTypeOfConst(iConstType), dEqual);  //create new cConstraint with m_dEqual value
        //    else return new cConstraint(itemList, sTxtDisplaye, getTypeOfConst(iConstType), Convert.ToDouble(drNew["dMin"]), Convert.ToDouble(drNew["dMax"])); //create new cConstraint with min / max values
        //}//getNewConstraintFromDB

        //private double getConstValue(object objVal)
        //{ // Returns double value (if exists), returns maximal double value otherwise
        //    try { return Convert.ToDouble(objVal); }
        //    catch (Exception ex) { }
        //    return double.MaxValue;
        //}//getConstValue

        #endregion Save / Load

        #region Security Index retrieval

        private String getStringOfConstraintSecs(cConstraint cConst, Boolean isInverse, cSecurities cSecsCol)
        { // Returns a String representing the securities active in current constraint (from all secs)
            m_arrConstItemsState = getArrOfConstraintSecs(cConst, cSecsCol);
            String strFinal = "";
            if (m_arrConstItemsState.Length == 0) return "";
            for (int iSecs = 0; iSecs < m_arrConstItemsState.Length; iSecs++)
            {
                if (isInverse) strFinal += (m_arrConstItemsState[iSecs] * -1).ToString() + ",";
                else strFinal += m_arrConstItemsState[iSecs].ToString() + ",";
            }
            strFinal = cGeneralFunctions.cropStrEnding(strFinal, 1);
            return strFinal;
        }//getStringOfConstraintSecs

        private List<double> getDoubleOfConstraintSecs(cConstraint cConst, Boolean isInverse, ISecurities cSecsCol)
        { // Returns a List representing the securities active in current constraint (from all secs)
            m_arrConstItemsState = getArrOfConstraintSecs(cConst, cSecsCol);
            List<double> doubleFinal = new List<double>();

            if ((m_arrConstItemsState == null) || (m_arrConstItemsState.Length == 0)) return null;
            for (int iSecs = 0; iSecs < m_arrConstItemsState.Length; iSecs++)
            {
                if (isInverse) doubleFinal.Add(m_arrConstItemsState[iSecs] * -1);
                else doubleFinal.Add(m_arrConstItemsState[iSecs]);
            }

            return doubleFinal;
        }//getDoubleOfConstraintSecs

        private int[] getArrOfConstraintSecs(cConstraint cConst, ISecurities cSecsCol)
        { // Returns array of participating securities in given constraint
            int[] iFinalArr = new int[cSecsCol.Count];
            for (int iItems = 0; iItems < cConst.ItemIds.Count; iItems++)
            { // Goes through constraint items
                iFinalArr = updtArrOfIndexes(iFinalArr, enumConstraintType.Security, cConst.ItemIds[iItems], iItems, cSecsCol);
                if (iFinalArr == null) return iFinalArr;
            }

            return iFinalArr;
        }//getArrOfConstraintSecs

        private int[] updtArrOfIndexes(int[] indArr, enumConstraintType eConstType, String itemId, int iStartPos, ISecurities cSecsCol)
        { // Updates array of indexes based on found securities
            for (int iSecs = iStartPos; iSecs < cSecsCol.Count; iSecs++)
                if (itemId == cSecsCol[iSecs].Properties.PortSecurityId)
                { indArr[iSecs] = 1; return indArr; }
            return null;
        }//updtArrOfIndexes

        private enumConstraintType getTypeOfConst(int constType)
        {//return the enumConstraintType value accordingly to the current constraint
            switch (constType)
            {
                case 1: return enumConstraintType.Market;
                case 2: return enumConstraintType.SecType;
                case 3: return enumConstraintType.Industry;
            }
            return enumConstraintType.Security;
        }//getTypeOfConst

        private String getStringOfIndSecs(double[] arrVals)
        { // Returns a String representing the securities active in current constraint (from all secs)
            String strFinal = "";
            if (arrVals.Length == 0) return "];";
            for (int iSecs = 0; iSecs < arrVals.Length; iSecs++)
                strFinal += arrVals[iSecs].ToString() + ";";
            strFinal = cGeneralFunctions.cropStrEnding(strFinal, 1) + "];";
            return strFinal;
        }//getStringOfIndSecs

        //private List<String> getListOfItems(int idCons, cDbOleConnection oleDBConn)
        //{//return list of security that participant in current constraint
        //    List<String> colConstItems = new List<String>();
        //    try
        //    {
        //        DataTable dtConstItems = cDbOleConnection.FillDataTable(cSqlStatements.getTblValsByIntConditionSQL(cDbOleConnection.TblConstraintItems, "idConstraint", idCons), oleDBConn.dbConnection);
        //        for (int iRows = 0; iRows < dtConstItems.Rows.Count; iRows++)
        //            colConstItems.Add(dtConstItems.Rows[iRows]["idItem"].ToString());

        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    }
        //    return colConstItems;
        //}//getListOfItems

        #endregion Security Index retrieval

        #region Add / Remove Methods

        public void addNewConstraint(cConstraint cConst) { addNewConstraint(cConst, cConst.DisplayText); }//addNewConstraint
        public void addNewConstraint(cConstraint cConst, String txtDisplay)
        { // Converts item to constraint + add to grid and collections
            m_colConstraints.Add(cConst); // Add to collection
            addNewConstToTable(cConst, txtDisplay);
        }//addNewConstraint

        private void addNewConstToTable(cConstraint cConst, String txtDisplay)
        { // Adds the specific constraint to the table displayed in control
            DataRow drNew = m_dtMainConsts.NewRow();
            if ((cConst.Minimum != double.MinValue) && (cConst.Maximum != double.MaxValue))
                if ((cConst.Maximum - cConst.Minimum) < 0.01D)
                    drNew["Vals"] = cGeneralFunctions.getFormatPercentFromDbl(cConst.Minimum, 2) + " - " + cGeneralFunctions.getFormatPercentFromDbl(cConst.Maximum, 4);
                else drNew["Vals"] = cGeneralFunctions.getFormatPercentFromDbl(cConst.Minimum, 2) + " - " + cGeneralFunctions.getFormatPercentFromDbl(cConst.Maximum, 2);
            if (cConst.Equality != double.MaxValue) drNew["Vals"] = cGeneralFunctions.getFormatPercentFromDbl(cConst.Equality, 2);

            drNew["Dist"] = (cConst.isOneSec) ? "Single" : "Totals";
            drNew["Items"] = txtDisplay;

            m_dtMainConsts.Rows.Add(drNew);
        }//addNewConstToTable

        public void removeConstraint(int index)
        { // Removes specific constraint from list and table
            m_colConstraints.RemoveAt(index);
            m_dtMainConsts.Rows.RemoveAt(index);
        }//removeConstraints

        public bool IsTotalPercentsGreaterThan100()
        { // Iterrate through all constraints
            double totalPers = 0;
            for (int i = 0; i < m_colConstraints.Count; i++)
            { // Check single and MINIMUM in ranges
                if (m_colConstraints[i].Equality != double.MaxValue)
                    totalPers += m_colConstraints[i].Equality;
                else if ((m_colConstraints[i].Minimum != double.MinValue) && (m_colConstraints[i].Maximum != double.MaxValue))
                    totalPers += m_colConstraints[i].Minimum;
            }

            if (totalPers > 1) return true;

            totalPers = 0;
            for (int i = 0; i < m_colConstraints.Count; i++)
            { // Check single and MAXIMUM in ranges
                if (m_colConstraints[i].Equality != double.MaxValue)
                    totalPers += m_colConstraints[i].Equality;
                else if ((m_colConstraints[i].Minimum != double.MinValue) && (m_colConstraints[i].Maximum != double.MaxValue))
                    totalPers += m_colConstraints[i].Maximum;
            }

            if (totalPers > 1) return true;

            return false;
        }//IsTotalLessThan100

        #endregion Add / Remove Methods

        #region Exportation of constraints

        #region Public method

        public void setConstraintValues(ISecurities cSecsCol)
        { // Creates all constraint string collections (matrices)
            try
            {
                refreshClassPointers();
                initSecBounds(cSecsCol);

                Double_setCollectionOfEqualityConsts(cSecsCol);
                Double_setCollectionOfRangeConsts(cSecsCol);

                setIndividualSecsConsts(cSecsCol);
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setConstraintValues

        #endregion Public method

        #region Equality

        private void Double_setCollectionOfEqualityConsts(ISecurities cSecsCol)
        { // Sets the collection of equality constraints suitable for scilab format
            // Default constraints
            m_colDefaultEquality.Clear();
            if (m_isSumTo100) addDefaultEqualitySumTo100(cSecsCol);

            // Empty constraint strings
            m_emptyDoubleEqualitySecs = new List<List<double>>();                     //c1
            m_emptyDoubleEqualityVals = new List<double>();                           //b1
            Double_addEqualityConstsFromCollection(m_colDefaultEquality, cSecsCol, true);

            // Fill constraint strings
            m_doubleEqualitySecs = new List<List<double>>();                          //c1
            m_doubleEqualityVals = new List<double>();                                //b1

            Double_addEqualityConstsFromCollection(m_colDefaultEquality, cSecsCol, false);
            Double_addEqualityConstsFromCollection(m_colEquality, cSecsCol, false);

            m_intEqualityNum = m_colEquality.Count + m_colDefaultEquality.Count;      // --me--
        }//Double_setCollectionOfEqualityConsts

        private void Double_addEqualityConstsFromCollection(cConstraints cConstCol, ISecurities cSecsCol, Boolean isEmpty)
        { // Adds the format of constraints from collection to final variables
            List<List<double>> colEqualitySecs = isEmpty ? m_emptyDoubleEqualitySecs : m_doubleEqualitySecs;
            List<double> colEqualityVals = isEmpty ? m_emptyDoubleEqualityVals : m_doubleEqualityVals;

            for (int iConst = 0; iConst < cConstCol.Count; iConst++)
            { // Inserts equality constraint
                List<double> row = getDoubleOfConstraintSecs(cConstCol[iConst], false, cSecsCol);               //c1

                if (row != null) colEqualitySecs.Add(row);
                else continue; // Skips constraint (in event failed to find corresponding securities

                colEqualityVals.Add(cConstCol[iConst].Equality);                                //b1
            }
        }//Double_addEqualityConstsFromCollection

        private void addDefaultEqualitySumTo100(ISecurities cSecsCol)
        { // Adds the equality constraint of all weights summing up to 100%
            List<String> lstAllSecs = new List<String>();
            for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
                lstAllSecs.Add(cSecsCol[iSecs].Properties.PortSecurityId);
            cConstraint cNewConst = new cConstraint(lstAllSecs, "", enumConstraintType.Security, 1D);
            m_colDefaultEquality.Add(cNewConst);
        }//addDefaultEqualitySumTo100

        #endregion Equality

        #region Inequality

        private void Double_setCollectionOfRangeConsts(ISecurities cSecsCol)
        { // Sets the collection of range constraints suitable for scilab format
            m_colDefaultRange.Clear();

            m_emptyDoubleRangeSecs = new List<List<double>>();
            m_emptyDoubleRangeVals = new List<double>();

            m_doubleRangeSecs = new List<List<double>>();
            m_doubleRangeVals = new List<double>();

            //if (m_is100OrLess) addDefaultRange100OrLess();
            Double_addRangeConstsFromCollection(m_colDefaultRange, cSecsCol);
            Double_addRangeConstsFromCollection(m_colRange, cSecsCol);
        }//Double_setCollectionOfRangeConsts

        private void Double_addRangeConstsFromCollection(cConstraints cConstCol, ISecurities cSecsCol)
        { // Adds the format of constraints from collection to final variables
            for (int iConst = 0; iConst < cConstCol.Count; iConst++)
            { // Inserts equality constraint
                if (cConstCol[iConst].Maximum != double.MaxValue) // Maximal value
                    Double_addSingleInequalityConstraint(cConstCol[iConst], false, true, cSecsCol);
                if (cConstCol[iConst].Minimum != double.MinValue) // Minimal value
                    Double_addSingleInequalityConstraint(cConstCol[iConst], true, false, cSecsCol);
            }//of main for
        }//Double_addRangeConstsFromCollection

        private void addDefaultRange100OrLess()
        { // Adds the default constraint of all securities sum to 100% or less
            List<String> lstAllSecs = new List<String>();
            for (int iSecs = 0; iSecs < m_objColHandler.ActiveSecs.Count; iSecs++)
                lstAllSecs.Add(m_objColHandler.ActiveSecs[iSecs].Properties.PortSecurityId);
            cConstraint cNewConst = new cConstraint(lstAllSecs, "", enumConstraintType.Security, 0D, 1D);
            m_colDefaultRange.Add(cNewConst);
        }//addDefaultRange100OrLess

        private void Double_addSingleInequalityConstraint(cConstraint cConst, Boolean isInverse, Boolean isMax, ISecurities cSecsCol)
        { // Inserts a single inequality constraint to string
            double dVal = (isMax) ? cConst.Maximum : -cConst.Minimum;
            List<double> row = getDoubleOfConstraintSecs(cConst, isInverse, cSecsCol);

            if (row != null)
                m_doubleRangeSecs.Add(row);

            m_doubleRangeVals.Add(dVal);
            //     --b2--
            if (!isMax && (dVal > 0))
                updtIndividualSecsValRange(-1D); // Only negative minimum value
        }//Double_addSingleInequalityConstraint

        private void updtIndividualSecsValRange(double dVal)
        { // Updates the range of the individual securities (only infimum values)
            for (int iSecs = 0; iSecs < m_arrConstItemsState.Length; iSecs++)
                if (m_arrConstItemsState[iSecs] == 1) // Only if security is participating in constraint
                    m_arrSecsLowerBound[iSecs] = dVal;
        }//updtIndividualSecsValRange

        #endregion Inequality

        #region Individual

        private void setIndividualSecsConsts(ISecurities cSecsCol)
        { // Sets the collection of constraints on individual securities

            // Empty lower / upper bounds
            m_arrEmptySecsLowerBound = new double[cSecsCol.Count];
            m_arrEmptySecsUpperBound = new double[cSecsCol.Count];
            for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
                m_arrEmptySecsUpperBound[iSecs] = 1D;

            // Full lower / upper bounds
            for (int iConsts = 0; iConsts < m_colSingleSecs.Count; iConsts++)
                for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
                    if (m_colSingleSecs[iConsts].ItemIds[0] == cSecsCol[iSecs].Properties.PortSecurityId)
                    { // Searches Security that matches SecurityID given in constraint
                        if (m_colSingleSecs[iConsts].Maximum != double.MaxValue) // Upper bound
                            m_arrSecsUpperBound[iSecs] = m_colSingleSecs[iConsts].Maximum;
                        if (m_colSingleSecs[iConsts].Minimum != double.MinValue) // Lower bound
                            m_arrSecsLowerBound[iSecs] = m_colSingleSecs[iConsts].Minimum;
                        break;
                    }//of for loops
        }//setIndividualSecsConsts

        #endregion Individual

        #endregion Exportation of constraints

        #endregion Methods

        #region Properties

        public cConstraints Constraints
        {
            get { return m_colConstraints; }
            set { m_colConstraints = value; }
        }//Constraints

        public DataTable MainConsts
        { get { return m_dtMainConsts; } }//MainConsts

        // UPPER / LOWER BOUNDS
        public double[] EmptyUpperBound
        { get { return m_arrEmptySecsUpperBound; } }//EmptyUpperBound
        public double[] DoubleUpperBound
        { get { return m_arrSecsUpperBound; } }//DoubleUpperBound

        public double[] EmptyLowerBound
        { get { return m_arrEmptySecsLowerBound; } }//EmptyLowerBound
        public double[] DoubleLowerBound
        { get { return m_arrSecsLowerBound; } }//DoubleLowerBound

        // EQUALITY CONSTRAINTS
        public List<List<double>> EmptyEqualitySecs //     --c1--  
        { get { return m_emptyDoubleEqualitySecs; } }//EmptyEqualitySecs
        public List<List<double>> DoubleEqualitySecs //    --c1--  
        { get { return m_doubleEqualitySecs; } }//DoubleEqualitySecs

        public double[] EmptyEqualityValues //     --b1--
        { get { return m_emptyDoubleEqualityVals.ToArray(); } }//EmptyEqualityValues
        public double[] DoubleEqualityValues //    --b1--
        { get { return m_doubleEqualityVals.ToArray(); } }//DoubleEqualityValues

        public int IntEqualityNum //--me--
        { get { return m_intEqualityNum; } }//IntEqualityNum

        // RANGE CONSTRAINTS
        public List<List<double>> Empty_NonEqualitySecs //  --c2-- 
        { get { return m_emptyDoubleRangeSecs; } }//Empty_NonEqualitySecs
        public List<List<double>> Double_NonEqualitySecs // --c2-- 
        { get { return m_doubleRangeSecs; } }//Double_NonEqualitySecs

        public double[] Empty_NonEqualityValues //--b2-- 
        { get { return m_emptyDoubleRangeVals.ToArray(); } }//Empty_NonEqualityValues
        public double[] Double_NonEqualityValues //--b2-- 
        { get { return m_doubleRangeVals.ToArray(); } }//Double_NonEqualityValues

        #endregion Properties

    }//of class
}
