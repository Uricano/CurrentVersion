using System;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;

//Used namespaces
using Cherries.TFI.BusinessLogic.Protection;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.TFI.BusinessLogic.General
{
    #region Error structure

    struct ErrorSturct
    {
        public string m_sFileName; // Class in which error was found
        public int m_iLineNum; // Line of error (in class)
        public string m_sErrorMsg; // Error message

        public string FullMessage // Returns the necessary error message
        { get { return "Error In " + m_sFileName + ". Line Number " + m_iLineNum.ToString() + ". Error Message" + m_sErrorMsg; } }//FullMessage

    }//ErrorSturct

    #endregion Error structure

    #region Error list class

    internal class ErrorList : List<ErrorSturct>
    {
        #region Methods

        //public void Add(string sMsg)
        //{ // Overloading the "Add" method of the base List<T> class
        //    ErrorSturct myErr = default(ErrorSturct);
        //    myErr.m_sFileName = "";
        //    myErr.m_iLineNum = 0;
        //    myErr.m_sErrorMsg = sMsg;

        //    base.Add(myErr);
        //}//Add

        #endregion Methods

    }//ErrorList class

    #endregion Error list class

    #region ErrorHandler calss

    public class cErrorHandler : IDisposable, IErrorHandler
    {
        #region Data Members

        // Main variables
        private ErrorList m_lstErrors = new ErrorList(); // Stacked list of accumulated errors
        private bool isDisposed = false; // indicates if Dispose has already been called

        // Data variables
        private string m_sLogFileName; // Log file name
        //private string m_sLogFileFolder; // Log file folder name
        private FileStream m_fsStreamer = null; // File streamer (manager)
        private StreamWriter m_swWriter = null; // File writer
        private int iErrorCnt = 0; // Counter of errors in current run

        // Constants
        //private readonly string m_cnstFileName = "Log.dat"; // Log default file name

        #endregion Data Members

        #region Constructors, Initialization & Destructor

        public cErrorHandler()
        { // Sets the name of the file + location
            //m_sLogFileFolder = cProperties.DataFolder;
            //m_sLogFileName = m_sLogFileFolder + "\\" + m_cnstFileName;
            m_sLogFileName = cProperties.DataFolder + "\\" + getLogFileName();
        }//constructor

        ~cErrorHandler()
        { Dispose(false); }//destructor

        protected void Dispose(bool disposing)
        { // Disposing class variables
            if (disposing)
            { // Managed code
                if (m_swWriter != null) m_swWriter.Close();
                if (m_fsStreamer != null) m_fsStreamer.Close();
                //if (m_lstErrors != null) m_lstErrors.Clear();
            }
            isDisposed = true;
        }//Dispose

        public void Dispose()
        { // indicates it was NOT called by the Garbage collector
            Dispose(true);
            GC.SuppressFinalize(this); // no need to do anything, stop the finalizer from being called
        }//Dispose

        private String getLogFileName()
        { return "Log_" + DateTime.Today.ToString("yyyy_MM") + ".dat"; }//getLogFileName

        #endregion Constructors, Initialization & Destructor

        #region Methods
        
        public void LogInfo(Exception ex)
        { //Writes error information to the log file including name of the file, 
            // line number & error message description
            try
            {
                m_sLogFileName = cProperties.DataFolder + "\\" + getLogFileName();

                System.Diagnostics.StackTrace errTrace = new System.Diagnostics.StackTrace(ex, true);
                ErrorSturct structError = new ErrorSturct();
                structError.m_sFileName = errTrace.GetFrame((errTrace.FrameCount - 1)).GetFileName();
                structError.m_iLineNum = errTrace.GetFrame((errTrace.FrameCount - 1)).GetFileLineNumber();
                structError.m_sErrorMsg = ex.Message;

                LogInfo(structError.FullMessage, errTrace);
    
                m_lstErrors.Add(structError);
            } catch (Exception exInner) {
                LogInfo(exInner.Message);
            }//try
        }//LogInfo

        public void LogInfo(string sMessage)
        { LogInfo(sMessage, new StackTrace()); }//LogInfo
        public void LogInfo(string sMessage, StackTrace errTrace)
        { // Adds information to the log file - the string is determined by the sInfo parameter
            try
            {
                setMainStreamer();
                //String strEncrypted = cAesSecurity.getEncryptedMessage(DateTime.Now + " " + sMessage, "andrew"); // Encrypt message
                String strNotEncrypted = DateTime.Now + " " + sMessage; // Encrypt message
                String strTrace = errTrace.ToString();
                m_swWriter.WriteLine(strNotEncrypted);
                if (strTrace.Trim() != "") m_swWriter.WriteLine(strTrace);
                m_swWriter.WriteLine();
                m_swWriter.Close();
                m_swWriter = null;
            } catch {
                iErrorCnt++; if (iErrorCnt > 1) { iErrorCnt = 0; return; } // Prevents endless loop if unable to write to file for some reason
                //LogInfo(Ex);
            }//try
        }//LogInfo

        private void setMainStreamer()
        { // Sets the main streamer file
            if ((m_fsStreamer != null) || (m_swWriter != null)) return; // Already been defined
            if (!File.Exists(m_sLogFileName))
            { // File doesn't exist - create!
                if (!Directory.Exists(cProperties.DataFolder))
                    Directory.CreateDirectory(cProperties.DataFolder);
                m_fsStreamer = File.Create(m_sLogFileName);
            } else m_fsStreamer = new FileStream(m_sLogFileName, FileMode.Append, FileAccess.Write); // Appending existing file

            m_swWriter = new StreamWriter(m_fsStreamer);
        }//setMainStreamer

        #endregion Methods

        #region Properties

        private string LogFileFullName
        { get { return m_sLogFileName; } }//LogFileFullName

        #endregion Properties

    }//class

    #endregion ErrorHandler class
}
