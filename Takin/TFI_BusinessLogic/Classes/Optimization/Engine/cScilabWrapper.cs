using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.IO;

using DotNetScilab;


// Used namespaces
using Cherries.TFI.BusinessLogic.General;

namespace DotNetScilab
{
    /// <summary>
    /// This class represents a wrapper for the scilab component
    /// It offers numerous mathematical computational capabilities
    /// </summary>
    public class cEngineWrapper
    {

        #region Data Members

        // Main variables
        private cScilab m_objEngine; // Main engine object
        private cErrorHandler m_objErrorHandler; // Error handler class

        // Matrices and vectors
        private double[] m_arrWorkMatrix; // dimensions of 1D array (as a 2D matrix = rows + columns)

        // General variables
        private Size m_sizeDimensions; // String representations of matrix columns

        #endregion Data Members

        #region Constructors, Initialization & Destructor

        public cEngineWrapper(cErrorHandler cErrHandler)
        {
            try
            {
                m_objErrorHandler = cErrHandler;
                
                //cEngineAssemblies cEng = new cEngineAssemblies(m_objErrorHandler);
                m_objEngine = new cScilab();
            } catch (Exception ex) {
                cErrHandler.LogInfo(ex);
            }
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region Matrix conversions

        public double[,] get1dTo2dArr(double[] origMatrix, Size matSize)
        { // Transforms a 1-dimensional array to a 2-dimensional matrix
            double[,] finalArr = null;
            finalArr = new double[matSize.Height, matSize.Width];
            int iPosVar = 0;
            int iPosVarCnt = 0;
            int iLen = 0;
            try
            {
                for (iLen = 0; iLen <= origMatrix.Length - 1; iLen++)
                {
                    if (iPosVarCnt == matSize.Height)
                    {
                        iPosVar += 1;
                        iPosVarCnt = 0;
                    }
                    finalArr[iPosVarCnt, iPosVar] = origMatrix[iLen];
                    iPosVarCnt += 1;
                }
                return finalArr;
            } catch (IndexOutOfRangeException exInd) {
                m_objErrorHandler.LogInfo(exInd.Message);
                return finalArr;
            }
        } // get1dTo2dArr

        #endregion Matrix conversions

        #region Command operations

        public void insertNewVectorDbl(string sVecName, double[] dVecData, bool isTranspose)
        { // Creates a new vector in scilab based on the given data array
            // TODO: In VB - isTranspose = false by default
            string strStruct = "[";
            string strSeperator = (isTranspose) ? ";" : " ";

            for (int iVals = 0; iVals <= dVecData.Length - 1; iVals++)
                strStruct += dVecData[iVals].ToString("#,###0.000") + strSeperator;
            
            String strCommand = sVecName + " = " + strStruct.Remove(strStruct.Length - 1) + "];";
            //m_frmMain.TabContainer.TabPages[0].Controls["memoScilabCmds"].Text += strCommand + System.Environment.NewLine;
            m_objEngine.SendScilabJob(strCommand);
        }//insertNewVectorDbl

        public void insertNewMatrixDbl(string sMatName, double[,] dMatData)
        { // Creates a new matrix in scilab based on the given 2D data array
            string strStruct = null, strCommand;
            sendEmptyMatrix(sMatName);
            //ObjScilab.SendScilabJob(sMatName + " = [];") ' Init matrix
            for (int iRows = 0; iRows <= dMatData.GetLength(0) - 1; iRows++)
            {
                strStruct = "";
                for (int iCols = 0; iCols <= dMatData.GetLength(1) - 1; iCols++)
                    strStruct += dMatData[iRows, iCols].ToString("#,###0.000") + ",";
                strCommand = sMatName + " = [" + sMatName + "; " + strStruct.Remove(strStruct.Length - 1) + "];";
                //m_frmMain.TabContainer.TabPages[0].Controls["memoScilabCmds"].Text += strCommand + System.Environment.NewLine;
                m_objEngine.SendScilabJob(strCommand);
                // Adds new row to matrix
            }
        }//insertNewMatrixDbl

        public void sendEmptyMatrix(string matName)
        { // Sends empty matrix command
            //m_frmMain.TabContainer.TabPages[0].Controls["memoScilabCmds"].Text += matName + " = [];" + System.Environment.NewLine;
            m_objEngine.SendScilabJob(matName + " = [];");
        }//sendEmptyMatrix

        public double[] getScilabMatrix(string matName)
        { // Returns a matrix loaded in the Scilab component
            return m_objEngine.readNamedMatrixOfDouble(matName);
        } //getScilabMatrix

        public void sendScilabCommand(String strCommand)
        { // Sends a command to scilab module + writes to log file
            //m_frmMain.TabContainer.TabPages[0].Controls["memoScilabCmds"].Text += strCommand + System.Environment.NewLine;
            m_objEngine.SendScilabJob(strCommand);
        }//sendScilabCommand

        public void clearCommandsTab()
        { // Clears the text currently found in tab
            //m_frmMain.TabContainer.TabPages[0].Controls["memoScilabCmds"].Text = "";
        }//sendCommandSeperatorToTab

        #endregion Command operations

        #endregion Methods

        #region Properties

        public cScilab Sc
        { get { return m_objEngine; } }//Sc

        #endregion Properties
    }
}