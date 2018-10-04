using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Net.Mail;
using System.Text.RegularExpressions;


// Used namespaces
using Cherries.TFI.BusinessLogic.DataManagement;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.Protection.LicenseManagement;
using System.Drawing.Imaging;
using System.IO;
using Cherries.TFI.BusinessLogic.Securities;

namespace Cherries.TFI.BusinessLogic.General
{
    public static class cGeneralFunctions
    {

        #region String operations

        public static String cropStrEnding(String strOrig, int iLen)
        { // Cuts the ending from a given string 
            return strOrig.Substring(0, strOrig.Length - iLen);
        }//cropStrEnding

        public static String insteadValue(String strOrig, string strReplace)
        { // Returns given value instead of defined string 

            // If empty string return replace value
            if (strOrig == "")
                return strReplace;
            return strOrig;
        }//insteadValue

        #endregion String operations

        #region Format operations

        public static String getFormatPercentFromDbl(double dVal, int iDecimalPnts)
        { // Transforms a double number to a percentage String format
            String strFormat = "{0:0.";
            for (int iPnts = 0; iPnts < iDecimalPnts; iPnts++) strFormat += "#";
            return String.Format(strFormat + "%}", dVal);
        }//getStringFormatPercent

        public static String getDateFormatStr(DateTime dtVal, String strSeperator)
        { // Returns a string presentation of a date without the time values
            return String.Format("{0:dd" + strSeperator + "MM" + strSeperator + "yyyy}", dtVal);
        }//getDateFormatStr

        public static String getDateFormatFullStr(DateTime dtVal, String strSeperator)
        { // Retrieves full Date format for display
            return String.Format("{0:MMM dd" + strSeperator + "yy}", dtVal);
        }//getDateFormatFullStr

        public static String getDateFormatStrForSQL(DateTime dtVal, String strSeperator)
        { // retrieves the String format of a date suitable for SQL querries
            // Format: YYYY/MM/DD
            return dtVal.Year.ToString() + strSeperator + dtVal.Month.ToString("00") + strSeperator + dtVal.Day.ToString("00");
        }//getStrFormatOfDate

        public static String getDoubleFormat(double dVal, int iDecimalPnts)
        { // Transforms a double number to a String format
            String strFormat = "{0:0.";
            for (int iPnts = 0; iPnts < iDecimalPnts; iPnts++) strFormat += "#";
            return String.Format(strFormat + "}", dVal);
        }//getFormatDoubleWithDecimal

        public static String isPasswordValid(String strPassword)
        { // Verifies the inserted password is acceptable
            String strMsgFinal = "";
            if (strPassword.Length > 15)
                strMsgFinal = "Password must not be longer than 15 characters";
            if (strPassword.Length < 8)
                strMsgFinal = "Password must not be shorter than 8 characters";
            if (Regex.IsMatch(strPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,15}$"))
                strMsgFinal = "Password must contain at least one digit, one lower case and one upper case rule";
            return strMsgFinal;
        }//isPasswordValid

        public static Boolean isMailAddressValid(String strMail)
        { // Verifies we have a valid e-mail address
            try
            {
                MailAddress m = new MailAddress(strMail); return true;
            } catch (FormatException) {
                return false;
            }
        }//isMailAddressValid

        #endregion Format operations

        #region Date operations

        public static double getPeriodYears(DateTime dtStart, DateTime dtEnd)
        { // Retrieves the interest rate for entirety of period
            TimeSpan tsRange = dtEnd - dtStart;
            return ((double)tsRange.Days / 365);
        }//getPerioTotalRates

        public static Boolean isBelowNumberOfMonths(cDateRange drPeriod, int iMonths)
        { if (((drPeriod.EndDate - drPeriod.StartDate).Days / 30) <= iMonths) return true; return false; }//isBelowNumberOfMonths

        public static Boolean isReturnsBelowNumberOfMonths(List<Models.App.PriceReturn> colReturns, int iMonths)
        { if (((double)colReturns.Count / 4D) <= iMonths) return true; else return false; }//isReturnsBelowNumberOfMonths

        public static string GetLocalDateFormat()
        { // Retrieves datetime format from computer's settings
            return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
        }//GetLocalDateFormat

        #endregion Date operations

        #region Currency operation

        public static string getCurrencyName(string calcCurrency, out string currencySign, out double adjCoeff)
        {
            if (calcCurrency == "9001")
            {
                adjCoeff = 1.0;
                currencySign = "$";
                return "USD";
            }
            if (calcCurrency == "9999")
            {
                adjCoeff = 100.0;   // Convert agorot into shekels
                currencySign = "₪";
                return "NIS";
            }
            adjCoeff = 1.0; 
            currencySign = "";
            return "";
        }//getCurrencyName

        
        
        
        #endregion Currency operation

        #region "Functions from Proper bonds"

        public static bool IsValueTooLarge(double value, double maxVal)
        {
            if (Convert.ToDouble(value) > maxVal || double.IsPositiveInfinity(Convert.ToDouble(value))) return true;
            return false;
        }//IsValueTooLarge

        #endregion "Functions from Proper bonds"

        #region Adjustment by CalcCurrency

        public static void AdjustPricesByCalcCurrency(DataTable dtHistoricalData, string calcCurrency, cErrorHandler m_objErrorHandler)
        {// Adjusts field fClose by Portfolio Calculation Currency
            DataRow dr;
            try
            {
                for (int i = 0; i < dtHistoricalData.Rows.Count; i++)
                {
                    dr = dtHistoricalData.Rows[i];
                    if (dr["idCurrency"].ToString() != calcCurrency)
                    {
                        if (calcCurrency == "9001")
                            dr["fClose"] = Convert.ToDouble(dr["fClose"]) / Convert.ToDouble(dr["USDPriceClose"]);
                        else if (calcCurrency == "9999")
                            dr["fClose"] = Convert.ToDouble(dr["fClose"]) * Convert.ToDouble(dr["USDPriceClose"]);
                    }
                }
            }
            catch (Exception ex)
            {
                m_objErrorHandler.LogInfo(ex);
            }
        }//AdjustPricesByCalcCurrency

        public static void AdjustPricesToExchangeCurrency(DataTable dtHistoricalData, string calcCurrency, cErrorHandler m_objErrorHandler)
        {// Adjusts field fClose by Portfolio Calculation Currency
            DataRow dr;
            try
            {
                for (int i = 0; i < dtHistoricalData.Rows.Count; i++)
                {
                    dr = dtHistoricalData.Rows[i];
                    if (dr["idCurrency"].ToString() != calcCurrency)
                    {
                        if (calcCurrency == "9001")
                            dr["fClose"] = Convert.ToDouble(dr["fClose"]) * Convert.ToDouble(dr["USDPriceClose"]);
                        else if (calcCurrency == "9999")
                            dr["fClose"] = Convert.ToDouble(dr["fClose"]) / Convert.ToDouble(dr["USDPriceClose"]);
                    }
                }
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//AdjustPricesByCalcCurrency

        public static string GetPriceFieldName(string calcCurrency)
        {// Assign field name for Price based on CalcCurrency
            
            if (calcCurrency == "9001")
                return "fClose";
            else if (calcCurrency == "9999")
                return "fNISClose";
                //return "fClose";

            return "";
        }//GetPriceFieldName

        public static double GetCurrentCurrencyRate(DataRow drRow)
        {
            // Later on it'll be not only for dual bonds, but all non-shekel currencies 
            // Adjusting amount for Dual Bond, have to divide by current LinkedBaseIndex rate
            bool IsBondDual = Convert.ToBoolean(drRow["IsBondDual"]);
            double CurrCurrencyRate = drRow["CurrCurrencyRate"] is DBNull ? 1 : (Convert.ToDouble(drRow["CurrCurrencyRate"]) / 100.0);
            //return (IsBondDual ? CurrCurrencyRate : 1);
            return (IsBondDual || drRow["Currency"].ToString() != "9999" ? CurrCurrencyRate : 1);
        }

        #endregion

       
        #region Graphic conversion

        public static string GetStringContainingImage(string strFilePath)
        {
            // Convert all images to .jpg

            // Get a bitmap.
            Bitmap bmp1 = new Bitmap(strFilePath);
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);

            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder myEncoder =
                System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L); //100L makes bigger file (pictures are same quality), 0L makes very low quality file
            myEncoderParameters.Param[0] = myEncoderParameter;

            MemoryStream ms = new MemoryStream();
            bmp1.Save(ms, jgpEncoder, myEncoderParameters);

            // Make a string out of .jpg image file
            byte[] myByteImage = ms.ToArray();                  

            string myStringImage = Convert.ToBase64String(myByteImage);
            return myStringImage;
        }//GetStringContainingImage

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        #endregion

        #region Securities

        public static string GetAdditionalSecurityInfo(cSecurities collSecurities, string prmSymbol, out string outStrSecID)
        {
            string outStrSecurityName = "";
            bool isUSA = false;
            outStrSecID = "";
            string symbol = "";
            int ind1 = prmSymbol.IndexOf("(");
            if(ind1 >= 0)
                symbol = prmSymbol.Substring(0, ind1).Trim();
            else
                symbol = prmSymbol.Trim();

            int ind2 = prmSymbol.IndexOf(")");
            string market = prmSymbol.Substring(ind1 + 1, ind2 - ind1 - 1);

            if(market == "USA")
                isUSA = true;

            for (int i = 0; i < collSecurities.Count; i++)
			{
                if((isUSA && (collSecurities[i].Properties.Market.ID == 3 || collSecurities[i].Properties.Market.ID == 4 || collSecurities[i].Properties.Market.ID == 5))
                   || (!isUSA && collSecurities[i].Properties.Market.ID == 1))
                {
                    if (collSecurities[i].Properties.SecuritySymbol == symbol)
                    {
                        outStrSecID = collSecurities[i].Properties.PortSecurityId;
                        outStrSecurityName = collSecurities[i].Properties.SecurityName;
                        break;
                    }
                }
			}

            return outStrSecurityName;
        }//GetAdditionalSecurityInfo

        #endregion


    }//of class
}
