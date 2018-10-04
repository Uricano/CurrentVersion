using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.IO;
using System.Reflection;
using System.Data;

using LogicNP.CryptoLicensing;

// Used namespaces
using Cherries.TFI_BusinessLogic.General;
using Cherries.TFI_BusinessLogic.GMath;
using Cherries.TFI_BusinessLogic.DataManagement;

namespace Cherries.TFI_BusinessLogic.Protection.LicenseManagement
{

    public class cLicManager
    {

        #region Data members

        // Main variables
        private cDbOleConnection m_objDbConnection; // DB connection class
        private cErrorHandler m_objErrorHandler = new cErrorHandler(); // Error handler
        private CryptoLicense m_objLicence; // License instance
        
        // General variables
        private Boolean m_isConnected = false; // Whether the connection to server has been established

        // Constant variables
        private const Boolean m_isAllowBuiltInLicense = true;
        private const String m_cnstLicCode = "tgIAAVeWjlDl0dEBAESo13Oo0gFaALIAUG9ydGZvbGlvUW50PTE7U2VjUGVyUG9ydGZvbGlvPTUwMDtpc1FBPUZhbHNlO0V4Y2hhbmdlcz0xLCAzLCA0LCA1O0V4dFBvcnRzUW50PTA7aXNUcmlhbD1GYWxzZTtCb25kT3B0aW1pemF0aW9uUW50PTA7T3B0aW1pemF0aW9uUW50PTA7aXNQcml2YXRlVXNlcj1UcnVlO0ZvbGxvd3VwUG9ydGZvbGlvc1FudD00ME06RWp+JhPIdm5Ok6VrvQTyyK6d9OdpvKYzz6KQR63yUnoUug9JOoqKKSkwojVQhA==";

        #endregion Data members

        #region Constructor

        public cLicManager(cErrorHandler cErrors, cDbOleConnection cConnection, Boolean isForm) 
        {
            m_objErrorHandler = cErrors;
            m_objDbConnection = cConnection;

            readLicenseFile();
            if (isForm) 
                loadLicenseLoginForm();
        }//constructor

        #endregion Constructor

        #region Methods

        #region General methods

        private void loadLicenseLoginForm()
        { // Loads the license login form (with login to server)
           
            //frmLicenseLoader fLicLoader = new frmLicenseLoader(this, m_objErrorHandler);
            //fLicLoader.ShowDialog();
            //m_frmLoginScreen = fLicLoader;
        }//loadLicenseLoginForm

        #endregion General methods

        #region License handling

        public void readLicenseFile()
        { readLicenseFile(cProperties.DataFolder + "\\" + Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location) + ".lic"); }//readLicenseFile

        public void readLicenseFile(String strFileName)
        { // Loads from license file all necessary information
            if (File.Exists(strFileName)) // License file exists - read from file
                setLicenseFromFile(strFileName);
            else { // License file doesn't exist - auto generated license
                if (!m_isAllowBuiltInLicense) return;

                m_objLicence = new CryptoLicense(m_cnstLicCode, cProperties.ProjectKey);
                m_objLicence.StorageMode = LicenseStorageMode.None;
            }

            cLicenseParams.CompID = m_objLicence.GetLocalMachineCodeAsString();
            cLicenseParams.setUserData(m_objLicence.UserData);
            cLicenseParams.DaysLeft = m_objLicence.RemainingUsageDays;
            cLicenseParams.DateExpires = m_objLicence.DateExpires;
            //cProperties.isWithQA = cLicenseParams.AllowQA;
            cProperties.NumBondOpts = cLicenseParams.BondOptNum;
            cProperties.NumOptimizations = cLicenseParams.OptimizationNum;
            //cProperties.isWithBondOpt = cLicenseParams.AllowBondOpt;
            //cProperties.isIsraelOnly = cLicenseParams.isIsraelOnly;
            setAllowedExchanges(cLicenseParams.Exchanges);
        }//loadFromFile

        private void setLicenseFromFile(String fName)
        { // Sets the license from the license file
            System.Text.ASCIIEncoding ansiEN = new System.Text.ASCIIEncoding();
            StreamReader streamReader = new StreamReader(fName, ansiEN);
            string lc = streamReader.ReadToEnd();

            m_objLicence = new CryptoLicense(lc, cProperties.ProjectKey);
            m_objLicence.StorageMode = LicenseStorageMode.None;
            m_objLicence.Load();
            streamReader.Close();
        }//setLicenseFromFile

        public void saveLicenseToFile(String strActivationCode)
        { // Saves license to file
            if (!Directory.Exists(cProperties.DataFolder)) Directory.CreateDirectory(cProperties.DataFolder);

            System.Text.ASCIIEncoding ansiEN = new System.Text.ASCIIEncoding();
            String strName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location) + ".lic";
            using (StreamWriter outfile = new StreamWriter(cProperties.DataFolder + "\\" + strName, false, ansiEN))
            {
                outfile.Write(strActivationCode);
                outfile.Close();
            }
        }//saveLicenseToFile

        private void setAllowedExchanges(String strExchanges)
        { // Sets the allowed exchanges for current user
            cProperties.AllowedExchanges.Clear();
            String[] arrExchanges = strExchanges.Split(',');

            if (arrExchanges.Length == 0)
            { setEmptyExchanges(); return; }

            for (int iExchanges = 0; iExchanges < arrExchanges.Length; iExchanges++)
                if (cMath.isNumericText(arrExchanges[iExchanges].Trim(), false))
                    cProperties.AllowedExchanges.Add(Convert.ToInt32(arrExchanges[iExchanges]));

            if ((cProperties.AllowedExchanges.Count == 0) || ((cProperties.AllowedExchanges.Count == 1) && cProperties.AllowedExchanges.Contains(1))) 
                setEmptyExchanges();
        }//setAllowedExchanges

        private void setEmptyExchanges()
        {  // If none found -> Israel only and exit
            cProperties.AllowedExchanges.Add(1);
            cProperties.isIsraelOnly = true;
        }//setEmptyExchanges

        #endregion License handling

        #endregion Methods

        #region Properties

        public cDbOleConnection Connection
        { get { return m_objDbConnection; } }//Connection

        public CryptoLicense License
        {
            get { return m_objLicence; }
            set { m_objLicence = value; }
        }//License

        public Boolean isValid
        { get { return (m_objLicence.Status == LicenseStatus.Valid); } }//isValid

        public Boolean isConnected
        {
            get { return m_isConnected; }
            set { m_isConnected = value; }
        }//isConnected

        #endregion Properties

    }//of class
}