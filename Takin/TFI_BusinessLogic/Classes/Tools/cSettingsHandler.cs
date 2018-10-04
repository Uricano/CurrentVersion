using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

// Used namespaces
using Cherries.TFI.BusinessLogic.General;

namespace Cherries.TFI.BusinessLogic.Tools
{
    public class cSettingsHandler
    {

        #region Data memebers

        // Main variables
        private cErrorHandler m_objErrorHandler; // Error handler class

        // Data variables
        private String m_strFileName = ""; // File name + path of INI file

        private const String m_cnstFileName = "AppSettings.dat"; // Ini file name (excluding folder)

        #endregion Data memebers

        #region Constructors, Initialization & Destructor

        public cSettingsHandler(cErrorHandler cErrors)
        {
            m_objErrorHandler = cErrors;

            try
            {
                setSettingsFileName();
                loadApplicationSettings();

            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//constructor

        private void setSettingsFileName()
        { // Sets the main file name
            m_strFileName = cProperties.DataFolder + "\\" + m_cnstFileName;
        }//setIniFileName

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region Initialization

        public void loadApplicationSettings()
        { // Loads application settings from file and sets cProperties accordingly
            try
            {


                verifyFileExists();
                //setSettingsCollection();
                setPropertiesFromFile();
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//loadApplicationSettings

        #endregion Initialization

        #region General methos

        private void verifyFileExists()
        { // Verifies settings file exists - if not creates it
            //try
            //{
            //    if (!File.Exists(m_strFileName))
            //    { // File doesn't exist - create!
            //        if (!isWithCurrencyId())
            //            setCurrencyValue();
            //        if (!Directory.Exists(cProperties.DataFolder)) Directory.CreateDirectory(cProperties.DataFolder);
            //        FileStream fsStreamer = File.Create(m_strFileName);
            //        fsStreamer.Close();
            //        buildNewSettingsFile(true); // Creates new - with default vals
            //    }
            //} catch (Exception ex) { m_objErrorHandler.LogInfo(ex); }
        }//verifyFileExists

        public void setPropertiesFromFile()
        { // Sets the default values read from the settings file to the current application thread
            try
            {
                


            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setAppDefaultValues

        #endregion General methos

        #endregion Methods

    }//of class
}
