using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Enums
{

    #region Price data enums

    public enum enumDateFreq
    {
        Weekly = 1,
        Daily = 2
    }//enumRateCalcFreq

    #endregion Price data enums

    #region General enums

    public enum enumSecurityDisplay
    { // Field of security to display
        Ticker = 1,
        Name = 2
    }//enumSecurityDisplay

    public enum enumCustomType
    { // Custom control mode
        CreatePortfolio = 1,
        EditPortfolio = 2
    }//enumCustomType

    public enum enumDataType
    {
        Percent = 1,
        Date = 2,
        Numeric = 3,
        String = 4, 
        Double = 5,
        Boolean = 6,
        Currency = 7
    }//enumDataType

    public enum enumConstraintType
    {
        Market = 1,
        SecType = 2,
        Industry = 3,
        Security = 4,
        Sector = 5
    }//enumConstraintType

    public enum enumMenuButtonState
    {
        Enabled = 1,
        Disabled = 2,
        Pressed = 3
    }//enumMenuButtonState

    public enum enumBacktestingSecTypes
    {
        Stock = 1,
        Benchmark = 2,
        Portfolio = 3
    }//enumBacktestingSecTypes

    #endregion General enums

    #region Calculation enums

    public enum enumEfCalculationType
    {
        BestTP = 1,
        BestRisk = 2,
        SecurityStd = 3,
        Custom = 4
    }//enumEfCalculationType

    #endregion Calculation enums

    #region Category enums

    public enum enumCatType
    {
        SecurityType = 1,
        StockMarket = 2,
        Sector = 4
    }//enumCatType

    public enum enumCatIdName
    {
        idSecurityType = 1,
        idStockMarket = 2,
        idCurrency = 3,
        idIndustry = 4,
        idSector = 5
    }//enumCatIdName

    public enum enumSettingsOptions
    {
        Nothing = 0,
        Frequency = 1,
        SecurityDisplay = 2,
        RiskFree = 3,
        ProprietaryRate = 4
    }//enumSettingsOptions

    #endregion Category enums

    public enum enumRiskLevel
    {
        None = 0,
        Solid = 1,
        Low = 2,
        Moderate = 3,
        High = 4,
        VeryHigh = 5
    }//RiskLevel

    public class cEnumHandler
    {

        #region Price data enum methods

        public static enumDateFreq getEnumDateFreq(int iDbVal)
        { // Translates Db numeric value to Enum val
            switch (iDbVal)
            { case 2: return enumDateFreq.Daily; }
            return enumDateFreq.Weekly; // Default value
        }//getEnumDateFreq

        #endregion Price data enum methods

        #region General enum methods

        public static enumSecurityDisplay getSecurityDisplayVal(String strVal)
        { // Returns security display for a given string value
            switch (strVal)
            {
                case "Ticker": return enumSecurityDisplay.Ticker;
                default: return enumSecurityDisplay.Name;
            }
        }//enumSecurityDisplay

        public static String getButtonStatePrefix(enumMenuButtonState enState)
        { // Returns the file prefix for the given button state
            switch (enState)
            {
                case enumMenuButtonState.Enabled: return "mnuEnabled";
                case enumMenuButtonState.Disabled: return "mnuDisabled";
                default: return "mnuPressed";
            }
        }//getButtonStatePrefix

        #endregion General enum methods

    }//of class
}
